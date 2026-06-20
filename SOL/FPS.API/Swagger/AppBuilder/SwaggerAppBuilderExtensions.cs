



using Swashbuckle.AspNetCore.SwaggerUI;

namespace Template.API.Swagger.AppBuilder;

public static class SwaggerAppBuilderExtensions
{
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, Action<SwaggerUIOptions>? setupAction = null)
        => app.UseSwagger()
            .UseSwaggerUI(setupAction);
}