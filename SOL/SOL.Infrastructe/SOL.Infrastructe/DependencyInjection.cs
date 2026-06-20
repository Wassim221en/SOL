using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOL.Application.Common.Interfaces;
using Template.Infrastructe.Services;

namespace Template.Infrastructe;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructe(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ISignalRService, SignalRService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFcmService, FcmService>();
        services.AddHttpClient<IDiscordNotifier, DiscordNotifier>();
        return services;
    }
}
