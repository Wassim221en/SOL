using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Primitives.Entity.Identity;

public class BaseRole : IdentityRole<Guid>, IBaseEntity<Guid>
{
    
    private List<IdentityUserRole<Guid>> _userRoles = new();
    public IReadOnlyCollection<IdentityUserRole<Guid>> UserRoles => _userRoles.AsReadOnly();

    private List<IdentityRoleClaim<Guid>> _roleClaims = new();
    public IReadOnlyCollection<IdentityRoleClaim<Guid>> RoleClaims => _roleClaims.AsReadOnly();
    public Guid? DeletedBy { get; set; }
    public DateTimeOffset? DateDeleted { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? DateUpdated { get; set; }
    public Guid? UpdatedBy { get; set; }

    public BaseRole(string name)
    {
        Name = name;
        NormalizedName = name.ToUpper();
    }

    public void ClearAllUsers()
        => _userRoles.Clear();

    public void AddUser(Guid userId)
        => _userRoles.Add(new IdentityUserRole<Guid>() { UserId = userId });
    public void AddUsers(List<Guid> userIds)
    =>_userRoles.AddRange(userIds.Select(userId => new IdentityUserRole<Guid>() { UserId = userId }));
    public void ClearAllPermissions()
        => _roleClaims.RemoveAll(rc=>rc.ClaimType=="Permission");
    public void AddPermission(string permission)
        => _roleClaims.Add(new IdentityRoleClaim<Guid> {ClaimType="Permission",ClaimValue=permission});
    public void AddPermissions(List<string> permissions)
        => _roleClaims.AddRange(permissions.Select(permission => new IdentityRoleClaim<Guid> { ClaimType = "Permission", ClaimValue = permission }));
    public List<string> GetAllPermissions()
    =>_roleClaims.Where(rc=>rc.ClaimType=="Permission").Select(rc=>rc.ClaimValue??"").ToList();
    public void RemovePermissions(List<string> permissions)
    {
        _roleClaims.RemoveAll(rc =>
            rc.ClaimType == "Permission" &&
            rc.ClaimValue != null &&
            permissions.Contains(rc.ClaimValue));
    }



    
}