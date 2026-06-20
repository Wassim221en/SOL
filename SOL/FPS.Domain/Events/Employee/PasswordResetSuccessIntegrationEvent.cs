using Template.Domain.Events;

namespace FPS.Domain.Events.Employee;

public class PasswordResetSuccessIntegrationEvent : IntegrationEvent
{
    public string FullName { get; }
    public string Email { get; }

    public PasswordResetSuccessIntegrationEvent(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }
}
