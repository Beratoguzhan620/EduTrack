using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Ad gereklidir."), MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir."), MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir."), MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir."), MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır."), MaxLength(100, ErrorMessage = "Şifre en fazla 100 karakter olabilir."), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
