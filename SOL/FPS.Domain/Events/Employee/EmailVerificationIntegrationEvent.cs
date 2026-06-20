using Template.Domain.Events;

namespace FPS.Domain.Events.Employee;

public class EmailVerificationIntegrationEvent : IntegrationEvent
{
    public string Email { get; }
    public string ResetToken { get; }
    public string FullName { get; }

    public EmailVerificationIntegrationEvent(string email, string resetToken, string fullName)
    {
        Email = email;
        ResetToken = resetToken;
        FullName = fullName;
    }
}
