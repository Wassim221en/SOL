using Microsoft.AspNetCore.Mvc;
using Template.Application.Common.Core.Response;

namespace Template.API.Extensions;

public static class OperationResponseExtensions
{
    public static IActionResult ToActionResult(this OperationResponse response)
    {
        if (response.Success)
            return new OkObjectResult(response);

        return response.StatusCode.HasValue
            ? new ObjectResult(response) { StatusCode = (int)response.StatusCode.Value }
            : new BadRequestObjectResult(response);
    }

    public static IActionResult ToActionResult<T>(this OperationResponse<T> response)
    {
        if (response.Success)
            return new OkObjectResult(response);

        return response.StatusCode.HasValue
            ? new ObjectResult(response) { StatusCode = (int)response.StatusCode.Value }
            : new BadRequestObjectResult(response);
    }
}
