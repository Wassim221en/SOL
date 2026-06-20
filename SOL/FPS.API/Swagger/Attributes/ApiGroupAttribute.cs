using Microsoft.AspNetCore.Mvc;

namespace Template.API.Attributes;

public class ApiGroupAttribute : ApiExplorerSettingsAttribute
{
    public ApiGroupAttribute()
    {
        
    }
    public ApiGroupAttribute(params string[] groupsNames)
    {
        GroupName = string.Join(",", groupsNames);
    }
}