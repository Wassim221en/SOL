using System.Net;
using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Exceptions.Http;

namespace FPS.Dashboard.Dashboard.Features.Role.Queries.GetById;

public class GetRoleByIdHandler : IRequestHandler<GetRolebyIdQuery.Request, OperationResponse<GetRolebyIdQuery.Response>>
{
    private readonly IRepository _repository;

    public GetRoleByIdHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationResponse<GetRolebyIdQuery.Response>> Handle(GetRolebyIdQuery.Request request, CancellationToken cancellationToken)
    {
        var role = await _repository.Query<FPS.Domain.Entities.Security.Role>()
            .Where(r => r.Id == request.Id)
            .Select(GetRolebyIdQuery.Response.Selector())
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
            return new HttpMessage("الدور غير موجود.", HttpStatusCode.NotFound);

        return role;
    }
}