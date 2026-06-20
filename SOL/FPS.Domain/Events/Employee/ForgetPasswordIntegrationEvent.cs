using Template.Domain.Events;

namespace FPS.Domain.Events.Employee;

public class ForgetPasswordIntegrationEvent : IntegrationEvent
{
    public string FullName { get; }
    public string ResetToken { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }

    public ForgetPasswordIntegrationEvent(string fullName, string resetToken, string? email, string? phoneNumber)
    {
        FullName = fullName;
        ResetToken = resetToken;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}
