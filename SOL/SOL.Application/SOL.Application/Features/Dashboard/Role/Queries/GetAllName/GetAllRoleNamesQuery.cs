using System.Linq.Expressions;
using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetAllName;

public class GetAllRoleNamesQuery
{
    public class Request : IRequest<OperationResponse<List<Response>>>
    {
    }

    public class Response
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }

        public static Expression<Func<SOL.Domain.Entities.Security.Role, Response>> Selector() => r => new()
        {
            RoleId = r.Id,
            RoleName = r.Name ?? "",
        };
    }
}

