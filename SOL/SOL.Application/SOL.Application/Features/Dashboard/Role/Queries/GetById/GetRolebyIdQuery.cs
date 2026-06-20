using System.Linq.Expressions;
using MediatR;
using Template.Domain.Enums;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetById;

public class GetRolebyIdQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
        public Guid Id { get; set; }
    }

    public class Response
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public RoleStatus Status { get; set; }

            public static Expression<Func<SOL.Domain.Entities.Security.Role, Response>> Selector() => r => new()
        {
            RoleId = r.Id,
            RoleName = r.Name ?? "",
            Description = r.Description,
            Status = r.Status,
        };
    }
}