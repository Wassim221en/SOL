using Swashbuckle.AspNetCore.Filters;
using Template.Application.Common.Core.Response;

namespace Template.API.Swagger;

public class OperationResponseExample : IExamplesProvider<OperationResponse>
{
    public OperationResponse GetExamples()
    {
        return new OperationResponse(true, null);
    }
}

public class OperationResponseExample<T> : IExamplesProvider<OperationResponse<T>> where T : class, new()
{
    public OperationResponse<T> GetExamples()
    {
        return new OperationResponse<T>(true,new T(), null);
    }
}