using System.Net;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Exceptions.Http;

namespace FPS.Dashboard.Dashboard.Features.Role.Commands.Delete;

public class DeleteRolesHandler : IRequestHandler<DeleteRolesCommand.Request, OperationResponse>
{
    private readonly IRepository _repository;

    public DeleteRolesHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse> Handle(DeleteRolesCommand.Request request, CancellationToken cancellationToken)
    {
        var role = await _repository.TrackingQuery<FPS.Domain.Entities.Security.Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
            return new HttpMessage("الدور غير موجود.", HttpStatusCode.NotFound);

        _repository.SoftDelete(role);
        await _repository.SaveChangesAsync(cancellationToken);

        return OperationResponse.Ok();
    }
}