using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.CodeVerification;

public class CodeVerificationCommand
{
    public class Request:IRequest<OperationResponse>
    {
        public string ?Email { get; set; }
        public string ?PhoneNumber { get; set; }
        public string ResetToken { get; set; }
    }
    
}