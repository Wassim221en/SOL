using System.Linq.Expressions;
using SOL.Domain.Entities.Security;
using MediatR;

namespace SOL.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeesAsPdf;

public class GetEmployeesAsPdfQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
        public List<Guid> EmployeeIds { get; set; }
    }

    public class Response
    {
        public required byte[] PdfBytes { get; set; }
        public required string FileName { get; set; }

        public class EmployeeRes
        {
            public long Number { get; set; }
            public string FullName { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Status { get; set; }

            public static Expression<Func<AppUser, EmployeeRes>> Selector() => e => new()
            {
                Number = e.Number,
                FullName = e.FullName,
                UserName = e.UserName ?? "",
                Email = e.Email ?? "",
                PhoneNumber = e.PhoneNumber ?? "",
                Status = e.Status == ActiveStatus.Active ? "نشط" : "غير نشط",
            };
        }
    }
}