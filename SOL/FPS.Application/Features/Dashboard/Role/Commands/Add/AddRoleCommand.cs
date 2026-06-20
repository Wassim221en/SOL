using MediatR;
using Template.Domain.Enums;

namespace FPS.Dashboard.Dashboard.Features.Role.Commands.Add;

public class AddRoleCommand
{
    public class Request : IRequest<OperationResponse<FPS.Dashboard.Dashboard.Features.Role.Queries.GetAll.GetAllRolesQuery.Response.RoleRes>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RoleStatus Status { get; set; }
    }
}