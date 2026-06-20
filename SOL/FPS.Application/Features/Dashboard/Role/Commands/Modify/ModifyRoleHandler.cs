using System.Net;
using FPS.Dashboard.Dashboard.Features.Role.Queries.GetById;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Exceptions.Http;

namespace FPS.Dashboard.Dashboard.Features.Role.Commands.Modify;

public class ModifyRoleHandler : IRequestHandler<ModifyRoleCommand.Request, OperationResponse<GetRolebyIdQuery.Response>>
{
    private readonly IRepository _repository;

    public ModifyRoleHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetRolebyIdQuery.Response>> Handle(ModifyRoleCommand.Request request, CancellationToken cancellationToken)
    {
        var role = await _repository.TrackingQuery<FPS.Domain.Entities.Security.Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
            return new HttpMessage("الدور غير موجود.", HttpStatusCode.NotFound);

        var duplicate = await _repository.Query<FPS.Domain.Entities.Security.Role>()
            .AnyAsync(r => r.Name == request.RoleName && r.Id != request.RoleId, cancellationToken);
        if (duplicate)
            return new HttpMessage("اسم الدور موجود مسبقاً.", HttpStatusCode.BadRequest);

        role.UpdateDetails(request.RoleName, request.Description, request.Status);
        await _repository.SaveChangesAsync(cancellationToken);

        return await _repository.Query<FPS.Domain.Entities.Security.Role>()
            .Where(r => r.Id == request.RoleId)
            .Select(GetRolebyIdQuery.Response.Selector())
            .FirstAsync(cancellationToken);
    }
}