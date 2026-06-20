using FPS.Dashboard.Dashboard.Features.Employee.Queries.GetById;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Template.Dashboard.Employee.Queries.GetById;

public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery.Request,
    OperationResponse<GetEmployeeByIdQuery.Response>>
{
    private readonly IRepository _repository;

    public GetEmployeeByIdHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetEmployeeByIdQuery.Response>> Handle(GetEmployeeByIdQuery.Request request,
        CancellationToken cancellationToken)
    {
        var query = _repository.Query<AppUser>()
            .Include(e => e.UserRoles)
            .Where(e => e.Id == request.Id);

        var user = await query
            .Select(GetEmployeeByIdQuery.Response.Selector())
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return EmployeeErrors.NotFound();

        var userRoleIds = await query.SelectMany(q => q.UserRoles).Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken: cancellationToken);

        user.Roles = await _repository.Query<FPS.Domain.Entities.Security.Role>()
            .Where(r => userRoleIds.Contains(r.Id))
            .Select(r => new GetEmployeeByIdQuery.Response.RoleRes()
            {
                RoleId = r.Id,
                Name = r.Name ?? ""
            }).ToListAsync(cancellationToken: cancellationToken);

        return user;
    }
}

