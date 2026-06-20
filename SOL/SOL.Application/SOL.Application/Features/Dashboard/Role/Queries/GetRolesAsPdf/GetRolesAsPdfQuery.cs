using System.Linq.Expressions;
using MediatR;
using Template.Domain.Enums;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetRolesAsPdf;

public class GetRolesAsPdfQuery
{
    public class Request:IRequest<OperationResponse<Response>>
    {
        public List<Guid> RoleIds { get; set; }
    }
    public class Response
    {
        public required byte[] PdfBytes { get; set; }
        public required string FileName { get; set; }
        public class RoleRes
        {
            public Guid RoleId { get; set; }
            public string Description { get; set; }
            public string RoleName { get; set; }
            public RoleStatus Status { get; set; }

            public static Expression<Func<SOL.Domain.Entities.Security.Role, RoleRes>> Selector() => r => new()
            {
                RoleId=r.Id,
                RoleName=r.Name??"",
                Description = r.Description,
                Status=r.Status,
            };
        }
    }
}