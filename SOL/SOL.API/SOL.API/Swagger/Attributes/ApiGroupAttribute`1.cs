namespace Template.API.Attributes;

public class ApiGroupAttribute<TEnum> : ApiGroupAttribute where TEnum : Enum
{
    public ApiGroupAttribute()
    {
        
    }
    public ApiGroupAttribute(params TEnum[] groupsNames)
    {
        GroupName = string.Join(",", groupsNames);
    }
}