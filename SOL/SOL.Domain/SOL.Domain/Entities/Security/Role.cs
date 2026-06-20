using Template.Domain.Enums;
using Template.Domain.Primitives.Entity.Identity;

namespace SOL.Domain.Entities.Security;

public class Role:BaseRole
{
    public Role(string name,string description, RoleStatus status) : base(name)
    {
        Status = status;
        Description = description;

    }
    
    public RoleStatus Status { get; private set; }
    public string Description { get; private set; }
    

    public static Role CreateRole(string name, string description, RoleStatus status, Guid? parentRoleId)
        => new Role(name, description, status);

    public void UpdateDetails(string name, string description, RoleStatus status)
    {
        Name = name;
        NormalizedName = name.ToUpper();
        Description = description;
        Status = status;
    }

    


}