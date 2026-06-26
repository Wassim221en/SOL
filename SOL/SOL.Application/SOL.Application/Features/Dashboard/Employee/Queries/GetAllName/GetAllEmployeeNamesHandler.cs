using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Employee.Queries.GetAllName;

public class GetAllEmployeeNamesHandler : IRequestHandler<GetAllEmployeeNamesQuery.Request, OperationResponse<List<GetAllEmployeeNamesQuery.Response>>>
{
    private readonly IRepository _repository;

    public GetAllEmployeeNamesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<List<GetAllEmployeeNamesQuery.Response>>> Handle(GetAllEmployeeNamesQuery.Request request, CancellationToken cancellationToken)
    {
        var query = _repository.Query<AppUser>();

        var employees = await query
            .OrderBy(e => e.FirstName + " " + e.LastName)
            .Select(GetAllEmployeeNamesQuery.Response.Selector())
            .ToListAsync(cancellationToken);

        return employees;
    }
}
