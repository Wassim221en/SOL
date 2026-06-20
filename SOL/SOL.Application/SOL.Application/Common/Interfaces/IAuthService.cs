using Template.Application.Common.Core.Response;

namespace SOL.Application.Common.Interfaces;

public class RefreshTokenResult
{
    public string? RefreshToken { get; set; }
    public Guid UserId { get; set; }
    public string? DeviceId { get; set; }
}

public interface IAuthService<TUser> where TUser : class
{
    string GenerateResetToken();
    string HashToken(string token);
    Task<string> GenerateAccessToken(TUser user, string? deviceId = null);
    Task<OperationResponse<string>> ForgetPasswordAsync(TUser user);
    Task<OperationResponse<RefreshTokenResult>> GenerateRefreshToken(Guid? userId, string? oldRefreshToken = null, CancellationToken cancellationToken = default);
    Task<OperationResponse<RefreshTokenResult>> GenerateRefreshToken(Guid? userId, string? oldRefreshToken = null, string? deviceId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
}
