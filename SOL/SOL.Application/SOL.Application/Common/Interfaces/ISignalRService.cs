using Template.Application.Common;

namespace SOL.Application.Common.Interfaces;

public interface ISignalRService
{
    Task SendNotificationToUser(string userId, string title, string message, string type = "info");
    Task SendNotificationToMultipleUsers(List<string> userIds, string title, string message, string type = "info");
    Task Send(string userId, string target, object message, HubType hubType);
    Task Send(string userId, string target, object message);
}
