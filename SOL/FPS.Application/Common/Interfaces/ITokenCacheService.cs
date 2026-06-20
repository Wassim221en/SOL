namespace FPS.Application.Common.Interfaces;

public interface ITokenCacheService
{
    Task InvalidateAllUserTokensAsync(string userId);
}
