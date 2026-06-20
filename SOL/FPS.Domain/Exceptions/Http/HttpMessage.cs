using System.Net;
using System.Text.Json.Serialization;

namespace Template.Domain.Exceptions.Http;

public class HttpMessage : IHttpMessage
{
    public HttpMessage(string message,HttpStatusCode statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
    [JsonIgnore] public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    //public bool IsSuccessStatusCode => StatusCode is >= HttpStatusCode.OK and <= (HttpStatusCode)299;
}