using System.Net;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Employee.Queries.GetEmployeeCardAsPdf;

public class GetEmployeeCardAsPdfHandler
    : IRequestHandler<GetEmployeeCardAsPdfQuery.Request, OperationResponse<GetEmployeeCardAsPdfQuery.Response>>
{
    private readonly IRepository _repository;
    private readonly IPdfService _pdfService;

    public GetEmployeeCardAsPdfHandler(IRepository repository, IPdfService pdfService)
    {
        _repository = repository;
        _pdfService = pdfService;
    }

    public async Task<OperationResponse<GetEmployeeCardAsPdfQuery.Response>> Handle(
        GetEmployeeCardAsPdfQuery.Request request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate == default || request.EndDate == default)
            return new HttpMessage("تاريخ البداية وتاريخ النهاية مطلوبان.", HttpStatusCode.BadRequest);

        if (string.IsNullOrWhiteSpace(request.EmployeeNameEn))
            return new HttpMessage("اسم الموظف باللغة الإنكليزية مطلوب.", HttpStatusCode.BadRequest);

        if (request.EndDate < request.StartDate)
            return new HttpMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.", HttpStatusCode.BadRequest);

        var user = await _repository.Query<AppUser>()
            .Where(e => e.Id == request.EmployeeId)
            .Select(GetEmployeeCardAsPdfQuery.Response.EmployeeCardDto.Selector())
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return EmployeeErrors.NotFound();

        var employeeNumber = user.Number.ToString();

        var bytes = await _pdfService.GenerateEmployeeCardPdf(
            user.FullName,
            request.EmployeeNameEn.Trim(),
            "",
            "",
            employeeNumber,
            request.StartDate,
            request.EndDate);

        return new GetEmployeeCardAsPdfQuery.Response
        {
            PdfBytes = bytes,
            FileName = $"UserCard-{user.FullName}-{DateTime.UtcNow.FromUtcToDamascus():yyyyMMddHHmmss}.pdf"
        };
    }
}
