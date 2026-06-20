namespace FPS.Application.Common.Core;

public class PermissionsPage
{
    public string Page { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
