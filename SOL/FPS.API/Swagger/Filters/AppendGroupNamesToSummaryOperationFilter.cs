using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Template.API.Filters;

public class AppendGroupNamesToSummaryOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.GroupName is not null)
        {
            operation.Summary += $" (Group Names: {context.ApiDescription.GroupName}.)";
        }
    }
}