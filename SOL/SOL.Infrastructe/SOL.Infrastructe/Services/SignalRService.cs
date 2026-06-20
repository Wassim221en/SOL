using Template.Application.Common;
using SOL.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using SignalR.Hubs.NotificationHub;

namespace Template.Infrastructe.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public async Task SendNotificationToUser(string userId, string title, string message, string type = "info")
        {
            var notification = new
            {
                Title = title,
                Message = message,
            };

            // Send notification only to users connected to NotificationHub
            if (ConnectedUsers.IsUserConnectedToHub(userId, HubType.Notification))
            {
                var connections = ConnectedUsers.GetUserConnectionsByHub(userId, HubType.Notification);
                foreach (var connectionId in connections)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                }
            }
        }
        public async Task SendNotificationToMultipleUsers(List<string> userIds, string title, string message, string type = "info")
        {
            foreach (var userId in userIds)
            {
                await SendNotificationToUser(userId, title, message, type);
            }
        }

        public async Task Send(string userId, string target, object message, HubType hubType)
        {
            var connections = ConnectedUsers.GetUserConnectionsByHub(userId, hubType);
            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(target, message);
            }
        }

        public async Task Send(string userId, string target, object message)
        {
            // Send to NotificationHub connections only
            if (ConnectedUsers.IsUserConnectedToHub(userId, HubType.Notification))
            {
                var connections = ConnectedUsers.GetUserConnectionsByHub(userId, HubType.Notification);
                foreach (var connectionId in connections)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync(target, message);
                }
            }
        }
    }
}

