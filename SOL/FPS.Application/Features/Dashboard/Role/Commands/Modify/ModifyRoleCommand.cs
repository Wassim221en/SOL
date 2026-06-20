using MediatR;
using Template.Domain.Enums;

namespace FPS.Dashboard.Dashboard.Features.Role.Commands.Modify;

public class ModifyRoleCommand
{
    public class Request : IRequest<OperationResponse<FPS.Dashboard.Dashboard.Features.Role.Queries.GetById.GetRolebyIdQuery.Response>>
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public RoleStatus Status { get; set; }
    }
}