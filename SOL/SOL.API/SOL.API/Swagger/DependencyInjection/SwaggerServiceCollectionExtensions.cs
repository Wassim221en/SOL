using Swashbuckle.AspNetCore.SwaggerGen;
using Template.API.Filters;

namespace Template.API.Swagger.DependencyInjection;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddNeptuneeSwagger(this IServiceCollection services, Action<SwaggerGenOptions>? options = null)
    {
        if (options is not null)
        {
            services.Configure(options);
        }

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.EnableAnnotations(true, true);

            // Use string names for enums instead of integer values
            o.UseInlineDefinitionsForEnums();
            o.SchemaFilter<EnumSchemaFilter>();

            // Custom schema ID generator to avoid conflicts
            o.CustomSchemaIds(type =>
            {
                if (type.IsGenericType)
                {
                    var genericTypeName = type.GetGenericTypeDefinition()
                        .Name
                        .Split('`')[0];

                    var genericArgs = string.Join("_", type.GetGenericArguments()
                        .Select(t => t.FullName?.Replace(".", "_")));

                    return $"{genericTypeName}_Of_{genericArgs}";
                }

                if (type.IsNested)
                    return $"{type.DeclaringType?.FullName?.Replace(".", "_")}_{type.Name}";

                return type.FullName?.Replace(".", "_");
            });


            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.xml");
            if (File.Exists(xmlPath))
            {
                o.IncludeXmlComments(xmlPath,true);
            }
        });

        return services;
    }


}
