using SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace SOL.Dashboard.Dashboard.Features.Employee.Commands.Add;

public class AddEmployeeCommand
{
    public class Request : IRequest<OperationResponse<GetAllEmployeesQuery.Response.EmployeeRes>>
    {
        // Basic Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<Guid> RoleIds { get; set; } = new();

        // Personal Information
        public Gender Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // Status
        public ActiveStatus ActiveStatus { get; set; }

        // Files
        public IFormFile? Image { get; set; }
    }
}