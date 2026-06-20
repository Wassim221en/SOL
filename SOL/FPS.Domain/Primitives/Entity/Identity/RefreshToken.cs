using System.ComponentModel.DataAnnotations.Schema;
using Template.Domain.Primitives.Entity.Base;

namespace Template.Domain.Primitives.Entity.Identity;

[Table ("RefreshTokens")]
public class RefreshToken:BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string RefreshTokenHash { get;set; }
    public DateTime ExpiresAt { get;set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get;set; }
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public RefreshToken(string refreshTokenHash, DateTime expiresAt,Guid userId) 
    {
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        IsUsed = false;
        IsRevoked = false;
        UserId = userId;
    }
    
    public void UseRefreshToken()
        => IsUsed = true;
    public void Revoke()
        => IsRevoked = true;

    public bool IsActive()
        => !IsUsed && !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public void UpdateDeviceInfo(string? deviceId, string? ipAddress)
    {
        DeviceId = deviceId;
        IpAddress = ipAddress;
    }

    public void Renew(string newHash, DateTime newExpiry)
    {
        RefreshTokenHash = newHash;
        ExpiresAt = newExpiry;
        IsUsed = false;
        IsRevoked = false;
    }

}