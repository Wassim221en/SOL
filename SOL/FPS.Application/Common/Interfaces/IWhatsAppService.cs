namespace FPS.Application.Common.Interfaces;

public interface IWhatsAppService
{
    Task SendMessageAsync(string phoneNumber, string message);
}
