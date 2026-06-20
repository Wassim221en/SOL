using Template.Domain.Entities.Base;

namespace Template.Domain.Entities.Notifications;

public class EmployeeNotification : Entity
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public bool IsRead { get; set; }

    public EmployeeNotification(Guid notificationId, Guid employeeId)
    {
        NotificationId = notificationId;
        EmployeeId = employeeId;
        IsRead = false;
    }

    public void MarkAsRead() => IsRead = true;
}
