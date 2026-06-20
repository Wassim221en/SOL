using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Role.Queries.GetAllName;

public class GetAllRoleNamesHandler : IRequestHandler<GetAllRoleNamesQuery.Request, OperationResponse<List<GetAllRoleNamesQuery.Response>>>
{
    private readonly IRepository _repository;

    public GetAllRoleNamesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<List<GetAllRoleNamesQuery.Response>>> Handle(GetAllRoleNamesQuery.Request request, CancellationToken cancellationToken)
    {
        var roles = await _repository.Query<FPS.Domain.Entities.Security.Role>()
            .OrderBy(r => r.Name)
            .Select(GetAllRoleNamesQuery.Response.Selector())
            .ToListAsync(cancellationToken);

        return roles;
    }
}

