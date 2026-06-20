using MediatR;
using Microsoft.AspNetCore.Http;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyProfile;

public class ModifyMyProfileCommand
{
    public class Request:IRequest<OperationResponse<Response>>
    {
        public string UserName { get; set; }
        public string?Email { get; set; }
        public IFormFile ? Image { get; set; }
        public bool DeleteImage { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? ImageUrl { get; set; }
    }
    
}