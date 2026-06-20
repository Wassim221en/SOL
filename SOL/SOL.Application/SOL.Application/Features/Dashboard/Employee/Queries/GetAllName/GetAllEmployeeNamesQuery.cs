using System.Linq.Expressions;
using SOL.Domain.Entities.Security;
using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAllName;

public class GetAllEmployeeNamesQuery
{
    public class Request : IRequest<OperationResponse<List<Response>>>
    {
    }

    public class Response
    {
        public Guid EmployeeId { get; set; }
        public long Number { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public static Expression<Func<AppUser, Response>> Selector() => e => new()
        {
            EmployeeId = e.Id,
            Number = e.Number,
            Name = $"{e.FirstName} {e.LastName}",
            PhoneNumber = e.PhoneNumber,
            ImageUrl = e.ImageUrl.OriginalImageUrl,
            ThumbnailUrl = e.ImageUrl.ThumbnailUrl,
        };
    }
}