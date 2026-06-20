using FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.RefreshToken;

public record RefreshTokenCommand()
{
    public record Request:IRequest<OperationResponse<LoginCommand.Response>>
    {
        public string RefreshToken { get; set; }
    }

    public record Response
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public List<RoleRes>Roles { get; set; }
        public class RoleRes
        {
            public Guid RoleId { get; set; }
            public string RoleName { get; set; }
            public List<string?> Permissions { get; set; }
        }
    }
}

