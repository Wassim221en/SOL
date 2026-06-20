namespace FPS.Application.Common.Interfaces;

public interface IHttpContextService
{
    bool TryGetCurrentUserId(out Guid userId);
}
