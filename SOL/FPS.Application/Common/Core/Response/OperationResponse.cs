using System.Net;
using Template.Domain.Exceptions.Http;

namespace Template.Application.Common.Core.Response;

public class OperationResponse<T>:OperationResponse
{
    public T? Data { get; set; }
    public OperationResponse(bool success, T? data, HttpMessage? errorMessage):base(success,errorMessage)
    {
        Data = data;
    }

    public static OperationResponse<T> Ok(T data)
        => new OperationResponse<T>(true, data, null);
    public static OperationResponse<T> Fail(HttpMessage errorMessage)
        => new OperationResponse<T>(false, default, errorMessage);
    
    public static implicit operator OperationResponse<T>(HttpMessage errorMessage)
        => Fail(errorMessage);
    public static implicit operator OperationResponse<T>(T data)
        => Ok(data);
    
}

public class OperationResponse
{
    public HttpStatusCode ?StatusCode { get; set; }
    public bool Success { get; set; }
    public HttpMessage? ErrorMessage { get; set; }

    public OperationResponse(bool success, HttpMessage? errorMessage)
    {
        StatusCode=errorMessage?.StatusCode;
        Success = success;
        ErrorMessage = errorMessage;
    }
    public static OperationResponse Ok()
        => new OperationResponse(true,null);
    public static OperationResponse Fail(HttpMessage errorMessage)
        => new OperationResponse(false, errorMessage);
    public static implicit operator OperationResponse(HttpMessage errorMessage)
        => Fail(errorMessage);
    public static implicit operator OperationResponse(bool success=true)
        => Ok();
}