namespace SOL.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendWelcomeEmailAsync(string to, string employeeName);
    Task SendPasswordResetEmailAsync(string to, string resetToken, string employeeName);
    Task SendEmailVerificationCode(string email, string code, string employeeName);
    Task SendPasswordResetSuccessEmailAsync(string to, string employeeName);
}
