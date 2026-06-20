using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.ModifyMyPassword;

public class ModifyMyPasswordCommand
{
    public class Request:IRequest<OperationResponse>
    {
        public string?OldPassword { get; set; }
        public string?NewPassword { get; set; }
        public string?ConfirmNewPassword { get; set; }
    }
    

    
}