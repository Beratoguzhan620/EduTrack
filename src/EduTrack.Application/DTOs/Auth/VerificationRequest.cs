namespace EduTrack.Application.DTOs.Auth;

public class VerificationRequest
{
    public int UserId { get; set; }
    public string Code { get; set; }
}
