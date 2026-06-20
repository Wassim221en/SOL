using System.Net;

namespace Template.Domain.Exceptions.Http;

public  class HttpException : Exception, IHttpMessage
{
    public HttpException(IHttpMessage httpMessage) : this(httpMessage.Message,httpMessage.StatusCode)
    {
        
    }
    public HttpException(string message, HttpStatusCode httpStatusCode) : base(message)
    {
        StatusCode = httpStatusCode;
        Message = message;
    }

    public HttpStatusCode StatusCode { get; set; }
    public new string Message { get; set; }

    /*public static implicit operator int?(HttpException? http) => (int?)(http?.StatusCode);
    public static implicit operator HttpStatusCode(HttpException http) => http?.StatusCode ?? default;
    public static implicit operator string(HttpException http) => http.Message;*/

    public override string ToString() => Message;
}