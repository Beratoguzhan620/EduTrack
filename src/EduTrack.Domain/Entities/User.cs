using Core.Domain;

namespace EduTrack.Domain.Entities;

public class User : Entity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; } = false;

    /// <summary>True once the user has confirmed the e-mail address with the verification code.</summary>
    public bool EmailConfirmed { get; set; }
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationExpiresAt { get; set; }

    public virtual List<UserRole> UserRoles { get; set; } = default!;
    public virtual List<StreamPost> StreamPosts { get; set; } = default!;
}
