using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetRolesAsPdf;

public class GetRolesAsPdfHandler:IRequestHandler<GetRolesAsPdfQuery.Request,OperationResponse<GetRolesAsPdfQuery.Response>>
{
    private readonly IRepository _repository;
    private readonly IPdfService _pdfService;

    public GetRolesAsPdfHandler(IRepository repository, IPdfService pdfService)
    {
        _repository = repository;
        _pdfService = pdfService;
    }

    public async Task<OperationResponse<GetRolesAsPdfQuery.Response>> Handle(GetRolesAsPdfQuery.Request request, CancellationToken cancellationToken)
    {
        var roles = await _repository.Query<SOL.Domain.Entities.Security.Role>()
            .Where(r => request.RoleIds.Contains(r.Id))
            .Select(GetRolesAsPdfQuery.Response.RoleRes.Selector())
            .ToListAsync(cancellationToken: cancellationToken);
        var columns = new List<string>() { "RoleName", "Description", "Status" };
        var bytes = await _pdfService.GeneratePdf("الأدوار", columns, roles);
        return new GetRolesAsPdfQuery.Response()
        {
            PdfBytes = bytes,
            FileName = "RolesPdf-"+DateTime.UtcNow.FromUtcToDamascus()
        };
    }
}