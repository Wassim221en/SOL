using System.Linq.Expressions;
using FPS.Domain.Entities.Security;
using MediatR;

namespace FPS.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeeCardAsPdf;

public class GetEmployeeCardAsPdfQuery
{
    public class Request : IRequest<OperationResponse<Response>>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeNameEn { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class Response
    {
        public required byte[] PdfBytes { get; set; }
        public required string FileName { get; set; }

        public class EmployeeCardDto
        {
            public long Number { get; set; }
            public string FullName { get; set; }

            public static Expression<Func<AppUser, EmployeeCardDto>> Selector() => e => new()
            {
                Number = e.Number,
                FullName = e.FullName,
            };
        }
    }
}
