using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SOL.Dashboard.Dashboard.Features.Role.Queries.GetAll;

public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery.Request, OperationResponse<GetAllRolesQuery.Response>>
{
    private readonly IRepository _repository;

    public GetAllRolesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetAllRolesQuery.Response>> Handle(GetAllRolesQuery.Request request, CancellationToken cancellationToken)
    {
        var roles = await _repository.Query<SOL.Domain.Entities.Security.Role>()
            .Where(r => !request.Status.HasValue || request.Status == r.Status)
            .Select(GetAllRolesQuery.Response.RoleRes.Selector())
            .ToListAsync(cancellationToken);

        return new GetAllRolesQuery.Response
        {
            Count = roles.Count,
            Roles = roles
        };
    }
}