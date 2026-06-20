using Template.Application.Common.Core.Response;

namespace SOL.Application.Common.Interfaces;

public interface INotificationService
{
    Task<OperationResponse<Guid>> CreateNotification(
        string title,
        string body,
        List<Guid> employeeIds,
        List<Guid> roleIds,
        bool sendToAllEmployees);

    Task SendNotification(Guid notificationId);

    Task UpdateUnreadNotificationsByUserId(Guid userId);

    Task SendInstantNotification(
        string title,
        string body,
        List<Guid> employeeIds,
        List<Guid> roleIds,
        bool sendToAllEmployees,
        Dictionary<string, string?>? data = null);
}
