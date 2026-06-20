using System.Linq.Expressions;
using FPS.Domain.Entities.Security;
using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Queries.GetMyProfile;

public class GetMyProfileQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
    }

    public class Response
    {
        public Guid EmployeeId { get; set; }
        public long Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public static Expression<Func<AppUser, Response>> Selector() => e => new()
        {
            EmployeeId = e.Id,
            Number = e.Number,
            FirstName = e.FirstName,
            LastName = e.LastName,
            UserName = e.UserName ?? "",
            Email = e.Email ?? "",
            PhoneNumber = e.PhoneNumber ?? "",
            ImageUrl = e.ImageUrl.OriginalImageUrl,
            ThumbnailUrl = e.ImageUrl.ThumbnailUrl,
        };
    }
}