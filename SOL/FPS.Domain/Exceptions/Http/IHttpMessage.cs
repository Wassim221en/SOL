using System.Net;
using System.Text.Json.Serialization;

namespace Template.Domain.Exceptions.Http;

public interface IHttpMessage
{
    [JsonIgnore] public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
}