using System.Linq.Expressions;
using FPS.Domain.Entities.Security;
using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Queries.GetById;

public class GetEmployeeByIdQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
        public Guid Id { get; set; }
    }

    public class Response
    {
        // Basic Information
        public Guid EmployeeId { get; set; }
        public long Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public ActiveStatus Status { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public List<RoleRes> Roles { get; set; }

        // Personal Information
        public Gender Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // Files
        public string? ImageUrl { get; set; }

        public class RoleRes
        {
            public Guid RoleId { get; set; }
            public string Name { get; set; }
        }

        public static Expression<Func<AppUser, Response>> Selector() => e => new()
        {
            EmployeeId = e.Id,
            Number = e.Number,
            FirstName = e.FirstName,
            LastName = e.LastName,
            FullName = e.FullName,
            Email = e.Email ?? "",
            PhoneNumber = e.PhoneNumber ?? "",
            UserName = e.UserName ?? "",
            EmailConfirmed = e.EmailConfirmed,
            PhoneNumberConfirmed = e.PhoneNumberConfirmed,
            DateCreated = e.DateCreated,
            Gender = e.Gender,
            DateOfBirth = e.DateOfBirth,
            Status = e.Status,
            ImageUrl = e.ImageUrl.OriginalImageUrl,
        };
    }
}

