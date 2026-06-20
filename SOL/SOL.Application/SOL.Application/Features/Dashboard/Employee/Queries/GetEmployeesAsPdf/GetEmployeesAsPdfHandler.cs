using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeesAsPdf;

public class GetEmployeesAsPdfHandler:IRequestHandler<GetEmployeesAsPdfQuery.Request,OperationResponse<GetEmployeesAsPdfQuery.Response>>
{
    private readonly IRepository _repository;
    private readonly IPdfService _pdfService;

    public GetEmployeesAsPdfHandler(IRepository repository, IPdfService pdfService)
    {
        _repository = repository;
        _pdfService = pdfService;
    }

    public async Task<OperationResponse<GetEmployeesAsPdfQuery.Response>> Handle(GetEmployeesAsPdfQuery.Request request, CancellationToken cancellationToken)
    {
        var employees = await _repository.Query<AppUser>()
            .Where(e => request.EmployeeIds.Contains(e.Id))
            .Select(GetEmployeesAsPdfQuery.Response.EmployeeRes.Selector())
            .ToListAsync(cancellationToken: cancellationToken);
        var columns = new List<string>() { "FullName", "UserName", "PhoneNumber", "Status" };
        var bytes = await _pdfService.GeneratePdf("الموظفون", columns, employees);
        return new GetEmployeesAsPdfQuery.Response()
        {
            PdfBytes = bytes,
            FileName = "EmployeesPdf-"+DateTime.UtcNow.FromUtcToDamascus()
        };
    }
}