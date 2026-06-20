using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructe.Services;

namespace Template.Infrastructe;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructe(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository, Repository>();
        return services;
    }
}
