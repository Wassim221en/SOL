using MediatR;
using Microsoft.AspNetCore.Http;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyImage;

public class ModifyImageCommand
{
    public class Request:IRequest<OperationResponse<Response>>
    {
        public Guid EmployeeId { get; set; }
        public IFormFile? Image { get; set; }
        public bool DeleteImage { get; set; }
    }

    public class Response
    {
        public string?ImageUrl { get; set; }
        public string ?ThumbnailUrl { get; set; }
    }

    public class ImageUrls
    {
        public string? OriginalImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}