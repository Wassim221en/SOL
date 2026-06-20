using System.Net;
using SOL.Dashboard.Dashboard.Features.Role.Queries.GetAll;
using SOL.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Exceptions.Http;

namespace SOL.Dashboard.Dashboard.Features.Role.Commands.Add;

public class AddRoleHandler : IRequestHandler<AddRoleCommand.Request, OperationResponse<GetAllRolesQuery.Response.RoleRes>>
{
    private readonly IRepository _repository;

    public AddRoleHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetAllRolesQuery.Response.RoleRes>> Handle(AddRoleCommand.Request request, CancellationToken cancellationToken)
    {
        var exists = await _repository.Query<SOL.Domain.Entities.Security.Role>()
            .AnyAsync(r => r.Name == request.Name, cancellationToken);
        if (exists)
            return new HttpMessage("اسم الدور موجود مسبقاً.", HttpStatusCode.BadRequest);

        var role = SOL.Domain.Entities.Security.Role.CreateRole(request.Name, request.Description, request.Status, null);
        await _repository.AddAsync(role);
        await _repository.SaveChangesAsync(cancellationToken);

        return new GetAllRolesQuery.Response.RoleRes
        {
            RoleId = role.Id,
            RoleName = role.Name ?? "",
            Description = role.Description,
            Status = role.Status,
        };
    }
}