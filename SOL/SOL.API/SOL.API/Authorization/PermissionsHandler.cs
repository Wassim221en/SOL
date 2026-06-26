using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SOL.Domain.Entities.Security;
using Template.API.Authorization;

namespace Template.API.Authorization;

public class PermissionsHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRepository _repository;

    public PermissionsHandler(IRepository repository)
    {
        _repository = repository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var isOwner = context.User.FindFirst("IsOwner")?.Value == "true";
        if (isOwner)
        {
            context.Succeed(requirement);
            return;
        }
        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var userPermissions = await _repository.Query<Role>()
            .Where(r => roles.Contains(r.Name))
            .SelectMany(r => r.RoleClaims.Select(rc => rc.ClaimValue))
            .ToListAsync(CancellationToken.None);

        bool isAuthorized = requirement.Operator switch
        {
            PermissionOperator.And =>
                requirement.Permissions.All(p => userPermissions.Contains(p)),

            PermissionOperator.Or =>
                requirement.Permissions.Any(p => userPermissions.Contains(p)),

            _ => false
        };

        if (isAuthorized)
            context.Succeed(requirement);
    }
}
