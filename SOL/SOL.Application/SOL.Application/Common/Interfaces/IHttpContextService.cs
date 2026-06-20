namespace SOL.Application.Common.Interfaces;

public interface IHttpContextService
{
    Guid? GetCurrentUserId();
    bool TryGetCurrentUserId(out Guid userId);
    string? GetCurrentEmail();
    string? GetCurrentUserName();
    string? GetClaimValue(string claimType);
}
