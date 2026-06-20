using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Auth.Commands.EmailConfirmed;

public class EmailConfirmedCommand
{
    public class Request:IRequest<OperationResponse>
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}