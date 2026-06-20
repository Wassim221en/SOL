namespace SOL.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string name);
}
