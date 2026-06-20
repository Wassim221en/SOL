using Microsoft.AspNetCore.Http;

namespace SOL.Application.Common.Interfaces;

public interface IDiscordNotifier
{
    Task SendExceptionAsync(Exception ex, HttpContext context);
}
