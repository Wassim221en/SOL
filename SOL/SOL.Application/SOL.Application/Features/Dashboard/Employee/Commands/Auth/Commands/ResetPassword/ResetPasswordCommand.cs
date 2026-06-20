using SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.Login;
using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ResetPassword;

public record ResetPasswordCommand()
{
    public record Request : IRequest<OperationResponse<LoginCommand.Response>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}

