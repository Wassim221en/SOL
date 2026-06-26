using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;

public record LoginCommand()
{
    public record Request:IRequest<OperationResponse<Response>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string?UserName { get; set; }
        public string Password { get; set; }
        public string? DeviceToken { get; set; }
    }
    public record Response
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<RoleRes> Roles { get; set; }
        public class RoleRes
        {
            public Guid RoleId { get; set; }
            public string RoleName { get; set; }
            public List<PermissionsPage> PermissionsPages { get; set; }
        }
    }
}

