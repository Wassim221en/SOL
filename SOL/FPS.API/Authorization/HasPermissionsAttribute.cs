using Microsoft.AspNetCore.Authorization;

namespace Template.API.Authorization;

public enum PermissionOperator
{
    And,
    Or
}

public class HasPermissionsAttribute : AuthorizeAttribute
{
    public string[] Permissions { get; }
    public PermissionOperator Operator { get; }

    public HasPermissionsAttribute(
        PermissionOperator permissionOperator,
        params string[] permissions)
    {
        Operator = permissionOperator;
        Permissions = permissions;
        Policy = $"{permissionOperator}:{string.Join(",", permissions)}";
    } 
    public HasPermissionsAttribute(
        params string[] permissions)
    {
        Operator=PermissionOperator.Or;
        Permissions = permissions;
        Policy = $"{Operator}:{string.Join(",", permissions)}";
    }
}