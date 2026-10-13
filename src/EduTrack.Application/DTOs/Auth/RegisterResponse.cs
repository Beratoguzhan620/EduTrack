namespace EduTrack.Application.DTOs.Auth;

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>False when SMTP is not configured and the verification e-mail could not be sent.</summary>
    public bool VerificationEmailSent { get; set; }

    /// <summary>
    /// The generated code. Only surfaced by the web layer in the Development environment when
    /// no e-mail could be sent, so the flow can still be completed locally.
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;
}
