using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetAll;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Dashboard.Dashboard.Features.Employee.Queries.GetAll;

namespace Template.Dashboard.Employee.Queries.GetAll;

public class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesQuery.Request, OperationResponse<GetAllEmployeesQuery.Response>>
{
    private readonly IRepository _repository;

    public GetAllEmployeesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetAllEmployeesQuery.Response>> Handle(GetAllEmployeesQuery.Request request, CancellationToken cancellationToken)
    {
        var specification = new GetAllEmployeesSpecification(request);
        var query = _repository.Query<AppUser>()
            .Where(specification.Criteria);

        var employees = await query
            .OrderByDescending(e => e.Number)
            .ApplySort(request.Column, request.SortType)
            .Select(GetAllEmployeesQuery.Response.EmployeeRes.Selector())
            .ToListAsync(cancellationToken);

        return new GetAllEmployeesQuery.Response
        {
            Count = employees.Count,
            Employees = employees.ApplyPagination(request.PageSize, request.PageIndex)
        };
    }
}

