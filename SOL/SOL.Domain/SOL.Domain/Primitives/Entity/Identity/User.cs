using Microsoft.AspNetCore.Identity;
using Template.Domain.Enums;
using Template.Domain.Primitives.Entity.Interface;

namespace Template.Domain.Primitives.Entity.Identity;

public abstract class User : IdentityUser<Guid>, IBaseEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public List<string?> DeviceTokens { get; set; } = new();
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
    public ImageUrls ImageUrl { get; set; } = new();

    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; }
    private readonly List<IdentityUserClaim<Guid>> _userClaims = new();
    public ICollection<IdentityUserClaim<Guid>> UserClaims => _userClaims.AsReadOnly();
    public Guid? DeletedBy { get; set; }
    public DateTimeOffset? DateDeleted { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? DateUpdated { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? LastSeen { get; set; }

    private List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    private List<UserDevices> _userDevices = new();
    public IReadOnlyCollection<UserDevices> UserDevices => _userDevices.AsReadOnly();

    public void AddRefreshToken(RefreshToken refreshToken)
        => _refreshTokens.Add(refreshToken);

    public void RemoveRefreshToken(RefreshToken refreshToken)
        => _refreshTokens.Remove(refreshToken);

    public void ClearRefreshTokens()
        => _refreshTokens.Clear();

    public void SetResetPasswordToken(string token, DateTime expiry)
    {
        ResetPasswordToken = token;
        ResetPasswordTokenExpiry = expiry;
    }

    public void ClearResetPasswordToken()
    {
        ResetPasswordToken = null;
        ResetPasswordTokenExpiry = null;
    }

    public void UpdateImageUrls(string? originalImageUrl, string? thumbnailUrl)
    {
        ImageUrl.OriginalImageUrl = originalImageUrl;
        ImageUrl.ThumbnailUrl = thumbnailUrl;
    }

    public void ClearImageUrls()
    {
        ImageUrl.OriginalImageUrl = null;
        ImageUrl.ThumbnailUrl = null;
    }

    public bool IsResetPasswordTokenValid()
    {
        return !string.IsNullOrEmpty(ResetPasswordToken)
               && ResetPasswordTokenExpiry.HasValue
               && ResetPasswordTokenExpiry.Value > DateTime.UtcNow;
    }

    public bool ValidateResetPasswordToken(string hashedToken)
    {
        return ResetPasswordToken == hashedToken && IsResetPasswordTokenValid();
    }

    // UserDevices Management
    public void AddOrUpdateDevice(string deviceToken, string deviceId, DeviceType type)
    {
        // Deactivate all existing devices with the same deviceId
        var existingDevices = _userDevices.Where(d => d.DeviceId == deviceId).ToList();
        foreach (var device in existingDevices)
        {
            _userDevices.Remove(device);
        }

        // Add new device
        var newDevice = new UserDevices(deviceToken, deviceId, type, true)
        {
            UserId = this.Id
        };
        _userDevices.Add(newDevice);
    }

    public void DeactivateDevice(string deviceId)
    {
        var devices = _userDevices.Where(d => d.DeviceId == deviceId && d.IsActive).ToList();
        foreach (var device in devices)
        {
            device.IsActive = false;
        }
    }

    public void DeactivateAllDevices()
    {
        foreach (var device in _userDevices.Where(d => d.IsActive).ToList())
        {
            _userDevices.Remove(device);
            device.IsActive = false;
        }
    }
    public class ImageUrls
    {
        public string? OriginalImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}