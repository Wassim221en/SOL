using System.Linq.Expressions;
using SOL.Domain.Entities.Security;
using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAll;

public class GetAllEmployeesQuery
{
    public class Request : PagingParams, IRequest<OperationResponse<Response>>
    {
        public string? Search { get; set; }
        public ActiveStatus? Status { get; set; }
    }

    public class Response
    {
        public int Count { get; set; }
        public List<EmployeeRes> Employees { get; set; }

        public class EmployeeRes
        {
            public Guid EmployeeId { get; set; }
            public long Number { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string? ImageUrl { get; set; }
            public string? ThumbnailUrl { get; set; }
            public ActiveStatus Status { get; set; }

            public static Expression<Func<AppUser, EmployeeRes>> Selector() => e => new()
            {
                EmployeeId = e.Id,
                Number = e.Number,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                UserName = e.UserName ?? "",
                Email = e.Email ?? "",
                PhoneNumber = e.PhoneNumber ?? "",
                ImageUrl = e.ImageUrl.OriginalImageUrl,
                ThumbnailUrl = e.ImageUrl.ThumbnailUrl,
                Status = e.Status,
            };
        }
    }
}