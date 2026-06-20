using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.ForgetPassword;

public record ForgetPasswordCommand()
{
    public record Request:IRequest<OperationResponse>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

