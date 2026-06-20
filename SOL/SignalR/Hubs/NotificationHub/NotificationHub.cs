using Microsoft.AspNetCore.SignalR;
using Template.Application.Common;

namespace SignalR.Hubs.NotificationHub;

public class NotificationHub : Hub, INotificationHub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is null)
            return;

        ConnectedUsers.AddConnection(userId, Context.ConnectionId, HubType.Notification);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            ConnectedUsers.RemoveConnection(userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
