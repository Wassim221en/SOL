namespace SOL.Application.Common.Interfaces;

public interface ITokenCacheService
{
    Task SaveAccessTokenAsync(string userId, string jti, TimeSpan expiry, string? deviceId = null);
    Task InvalidateAllUserTokensAsync(string userId);
}
