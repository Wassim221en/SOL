using Template.Domain.Enums;

namespace SOL.Domain.Entities.Security;

public class AppUser : Template.Domain.Primitives.Entity.Identity.User
{
    // =======================
    // Basic Fields
    // =======================
    public long Number { get; set; }
    public Gender Gender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public ActiveStatus Status { get; private set; }

    // =======================
    // Domain Methods
    // =======================

    /// <summary>
    /// Updates basic user information (FirstName, LastName, Email, UserName, PhoneNumber)
    /// </summary>
    public void UpdateBasicInformation(
        string firstName,
        string lastName,
        string? email,
        string userName,
        string phoneNumber)
    {
        if (email is not null && email != Email)
            EmailConfirmed = false;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserName = userName;
        PhoneNumber = phoneNumber;
    }

    /// <summary>
    /// Updates personal information (Gender, DateOfBirth)
    /// </summary>
    public void UpdatePersonalInformation(Gender gender, DateOnly? dateOfBirth)
    {
        Gender = gender;
        DateOfBirth = dateOfBirth;
    }

    public void SetStatus(ActiveStatus status)
        => Status = status;

    public void UpdateImageUrl(string? originalImageUrl, string? thumbnailUrl = null)
        => UpdateImageUrls(originalImageUrl, thumbnailUrl);
}
