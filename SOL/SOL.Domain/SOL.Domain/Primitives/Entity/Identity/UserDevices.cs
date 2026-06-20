using Template.Domain.Enums;
using Template.Domain.Primitives.Entity.Identity;

namespace Template.Domain.Primitives.Entity.Identity;

public class UserDevices:Template.Domain.Entities.Base.Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string DeviceToken { get; set; }
    public string DeviceId { get; set; }
    public DeviceType Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastLogoutAt { get; set; }


    public UserDevices(string deviceToken, string deviceId, DeviceType type, bool isActive)
    {
        DeviceToken = deviceToken;
        DeviceId = deviceId;
        Type = type;
        IsActive = isActive;
        LastLoginAt = DateTime.UtcNow;
    }
    public UserDevices(Guid userId,string deviceToken, string deviceId, DeviceType type, bool isActive)
    {
        UserId = userId;
        DeviceToken = deviceToken;
        DeviceId = deviceId;
        Type = type;
        IsActive = isActive;
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the device and updates the last login timestamp
    /// </summary>
    public void Login()
    {
        IsActive = true;
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the device and updates the last logout timestamp
    /// </summary>
    public void Logout()
    {
        IsActive = false;
        LastLogoutAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the device token (e.g., when FCM token is refreshed)
    /// </summary>
    public void UpdateDeviceToken(string newDeviceToken)
    {
        DeviceToken = newDeviceToken;
        IsActive = true;
    }
}