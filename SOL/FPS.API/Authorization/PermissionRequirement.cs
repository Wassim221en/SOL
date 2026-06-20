using Template.API.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Template.API.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public PermissionOperator Operator { get; }

    public PermissionRequirement(
        PermissionOperator permissionOperator,
        string[] permissions)
    {
        Operator = permissionOperator;
        Permissions = permissions;
    }
}