using System.Linq.Expressions;
using MediatR;
using Template.Domain.Enums;

namespace FPS.Dashboard.Dashboard.Features.Role.Queries.GetAll;

public class GetAllRolesQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
        public RoleStatus? Status { get; set; }
    }

    public class Response
    {
        public int Count { get; set; }
        public List<RoleRes> Roles { get; set; }

        public class RoleRes
        {
            public Guid RoleId { get; set; }
            public string RoleName { get; set; }
            public string Description { get; set; }
            public RoleStatus Status { get; set; }

            public static Expression<Func<FPS.Domain.Entities.Security.Role, RoleRes>> Selector() => r => new()
            {
                RoleId = r.Id,
                RoleName = r.Name ?? "",
                Description = r.Description,
                Status = r.Status,
            };
        }
    }
}