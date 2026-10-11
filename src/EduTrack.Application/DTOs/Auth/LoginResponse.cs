namespace EduTrack.Application.DTOs.Auth;

public class LoginResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public List<string> Roles { get; set; } = default!;
}
