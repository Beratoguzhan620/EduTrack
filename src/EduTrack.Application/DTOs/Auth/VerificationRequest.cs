using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs.Auth;

public class VerificationRequest
{
    [Required(ErrorMessage = "E-posta gereklidir."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğrulama kodu gereklidir.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod 6 haneli olmalıdır.")]
    public string Code { get; set; } = string.Empty;
}
