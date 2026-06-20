using System.Net;
using Template.Application.Common;
using SOL.Application.Common.Interfaces;
using Template.Domain.Entities.Notifications;
using SOL.Domain.Entities.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.Hubs.GeneralHub;
using Template.Application.Common.Core.Response;
using Template.Domain.Exceptions.Http;

namespace Template.Infrastructe.Services;

public class NotificationService : INotificationService
{
    private readonly IRepository _repository;
    private readonly IFcmService _fcmService;
    private readonly ISignalRService _signalRService;
    private readonly IHubContext<GeneralHub> _generalHub;

    public NotificationService(IRepository repository, IFcmService fcmService, ISignalRService signalRService,
        IHubContext<GeneralHub> generalHub)
    {
        _repository = repository;
        _fcmService = fcmService;
        _signalRService = signalRService;
        _generalHub = generalHub;
    }

    /*public async Task SendNotification(Guid notificationId)
    {
        var notification = await _repository.TrackingQuery<Notification>()
            .Include(n => n.UserNotifications)
            .FirstOrDefaultAsync(n => n.Id == notificationId);
        if (notification is null)
            return;
        var listOfDeviceTokens = notification.UserNotifications
            .SelectMany(un=>un.Employee.DeviceTokens).Where(dt=>dt!=null).ToList();
        await _fcmService.SendNotificationToMultipleDevicesAsync(listOfDeviceTokens!, notification.Title,
            notification.Body);
        var listOfUserIds= notification.UserNotifications
            .Select(un => un.EmployeeId.ToString()).ToList();
        await _signalRService.SendNotificationToMultipleUsers(listOfUserIds, notification.Title,notification.Body);
    }*/

    public async Task<OperationResponse<Guid>> CreateNotification(
        string title,
        string body,
        List<Guid> employeeIds,
        List<Guid> roleIds,
        bool sendToAllEmployees)
    {
        try
        {
            var notification = new Notification(title, body, null);
            var targetEmployeeIds = new List<Guid>();

            if (sendToAllEmployees)
            {
                targetEmployeeIds = await _repository
                    .Query<AppUser>()
                    .Select(e => e.Id)
                    .ToListAsync();
            }
            else
            {
                if (roleIds != null && roleIds.Any())
                {
                    var employeesFromRoles = await _repository
                        .Query<Role>()
                        .Where(r => roleIds.Contains(r.Id))
                        .SelectMany(r => r.UserRoles.Select(ur => ur.UserId))
                        .Distinct()
                        .ToListAsync();

                    targetEmployeeIds.AddRange(employeesFromRoles);
                }

                if (employeeIds != null && employeeIds.Any())
                {
                    targetEmployeeIds.AddRange(employeeIds);
                }

                targetEmployeeIds = targetEmployeeIds.Distinct().ToList();
            }

            notification.AddUsers(targetEmployeeIds);

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();

            return OperationResponse<Guid>.Ok(notification.Id);
        }
        catch (Exception ex)
        {
            return OperationResponse<Guid>.Fail(new HttpMessage(ex.Message, HttpStatusCode.InternalServerError));
        }
    }

    public async Task SendNotification(Guid notificationId)
    {
        var notification = await _repository.Query<Notification>()
            .Include(n => n.UserNotifications)
            .FirstOrDefaultAsync(n => n.Id == notificationId);
        if (notification is null)
            return;
        var targetEmployeeIds = notification.UserNotifications.Select(un => un.EmployeeId).ToList();
        var employeesWithTokens = await _repository
            .Query<AppUser>()
            .Include(e => e.UserDevices)
            .Where(e => targetEmployeeIds.Contains(e.Id))
            .ToListAsync();

        var listOfDeviceTokens = employeesWithTokens
            .SelectMany(e => e.UserDevices.Select(ud => ud.DeviceToken))
            .Where(e => e is { Length: > 10 })
            .Where(dt => !string.IsNullOrWhiteSpace(dt))
            .Distinct()
            .ToList();

        // Send FCM notification
        if (listOfDeviceTokens.Any())
        {
            await _fcmService.SendNotificationToMultipleDevicesAsync(listOfDeviceTokens!, notification.Title,
                notification.Body);
        }

        // Send SignalR notification
        var listOfUserIds = targetEmployeeIds.Select(id => id.ToString()).ToList();
        await _signalRService.SendNotificationToMultipleUsers(listOfUserIds, notification.Title, notification.Body);
        foreach (var employeeId in targetEmployeeIds)
        {
            await UpdateUnreadNotificationsByUserId(employeeId);
        }
    }

    public async Task UpdateUnreadNotificationsByUserId(Guid userId)
    {
        var numberOfUnreadNotificationsByUserId = await _repository.Query<EmployeeNotification>()
            .CountAsync(e => e.EmployeeId == userId && !e.DateDeleted.HasValue && !e.IsRead);
        var connections = ConnectedUsers.GetUserConnectionsByHub(userId.ToString(), HubType.General);
        foreach (var connectionId in connections)
        {
            await _generalHub.Clients.Client(connectionId).SendAsync("UpdateUnreadNotificationsCount", ToString(),
                new
                {
                    UnreadNotificationsCount = numberOfUnreadNotificationsByUserId.ToString()
                });
        }
    }

    public async Task SendInstantNotification(
        string title,
        string body,
        List<Guid> employeeIds,
        List<Guid> roleIds,
        bool sendToAllEmployees,
        Dictionary<string, string?>? data = null)
    {
        var targetEmployeeIds = new List<Guid>();

        if (sendToAllEmployees)
        {
            targetEmployeeIds = await _repository
                .Query<AppUser>()
                .Select(e => e.Id)
                .ToListAsync();
        }
        else
        {
            if (roleIds != null && roleIds.Any())
            {
                var employeesFromRoles = await _repository
                    .Query<Role>()
                    .Where(r => roleIds.Contains(r.Id))
                    .SelectMany(r => r.UserRoles.Select(ur => ur.UserId))
                    .Distinct()
                    .ToListAsync();

                targetEmployeeIds.AddRange(employeesFromRoles);
            }

            if (employeeIds != null && employeeIds.Any())
            {
                targetEmployeeIds.AddRange(employeeIds);
            }

            targetEmployeeIds = targetEmployeeIds.Distinct().ToList();
        }

        if (!targetEmployeeIds.Any())
            return;
        var employeesWithTokens = await _repository
            .Query<AppUser>()
            .Include(e => e.UserDevices)
            .Where(e => targetEmployeeIds.Contains(e.Id))
            .ToListAsync();

        var deviceTokens = employeesWithTokens
            .SelectMany(e => e.UserDevices.Select(d => d.DeviceToken))
            .Where(t => !string.IsNullOrWhiteSpace(t) && t.Length > 10)
            .Distinct()
            .ToList();
        if (deviceTokens.Any())
        {
            await _fcmService.SendNotificationToMultipleDevicesAsync(
                deviceTokens,
                title,
                body,
                data);
        }

        var userIds = targetEmployeeIds.Select(id => id.ToString()).ToList();

        await _signalRService.SendNotificationToMultipleUsers(
            userIds,
            title,
            body);
    }
}