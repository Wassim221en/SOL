using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FPS.Dashboard.Dashboard.Features.Employee.Commands.Modify;

public class ModifyEmployeeCommand
{
    public class Request : IRequest<OperationResponse<GetEmployeeByIdQuery.Response>>
    {
        public Guid EmployeeId { get; set; }

        // Basic Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<Guid> RoleIds { get; set; } = new();

        // Personal Information
        public Gender Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // Status
        public ActiveStatus ActiveStatus { get; set; }

        // Files
        public IFormFile? Image { get; set; }
        public bool DeleteImage { get; set; }

        public string? NewPassword { get; set; }
    }
}

