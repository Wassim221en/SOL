using Template.Domain.Entities.Base;

namespace Template.Domain.Entities.Notifications;

public class Notification : Entity
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string? ImageUrl { get; set; }

    private readonly List<EmployeeNotification> _userNotifications = new();
    public IReadOnlyCollection<EmployeeNotification> UserNotifications => _userNotifications.AsReadOnly();

    public Notification(string title, string body, string? imageUrl)
    {
        Title = title;
        Body = body;
        ImageUrl = imageUrl;
    }

    public void AddUsers(List<Guid> employeeIds)
    {
        _userNotifications.AddRange(
            employeeIds.Select(id => new EmployeeNotification(Id, id)));
    }
}
