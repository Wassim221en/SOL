using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Role.Commands.Delete;

public class DeleteRolesCommand
{
    public class Request : IRequest<OperationResponse>
    {
       public Guid RoleId { get; set; }
    }
}

