using FPS.Domain.Entities.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FPS.Dashboard.Dashboard.Features.Employee.Queries.GetMyProfile;

public class GetMyProfileHandler:IRequestHandler<GetMyProfileQuery.Request,OperationResponse<GetMyProfileQuery.Response>>
{
    private readonly IRepository _repository;
    private readonly IHttpContextService _httpContextService;
    public GetMyProfileHandler(IRepository repository, IHttpContextService httpContextService)
    {
        _repository = repository;
        _httpContextService = httpContextService;
    }

    public async Task<OperationResponse<GetMyProfileQuery.Response>> Handle(GetMyProfileQuery.Request request, CancellationToken cancellationToken)
    {
        if (!_httpContextService.TryGetCurrentUserId(out Guid userId))
            return EmployeeErrors.UnAuthenticated();
        var user = await _repository.Query<AppUser>()
            .Where(e => e.Id == userId).Select(GetMyProfileQuery.Response.Selector())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user is null)
            return EmployeeErrors.NotFound();
        return user;

    }
}