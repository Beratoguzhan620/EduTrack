using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.Application.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}


public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email adresi gereklidir.")
            .EmailAddress()
            .WithMessage("Lütfen geçerli bir email adresi giriniz.")
            .MaximumLength(100)
            .WithMessage("Email adresi en fazla 100 karakter olabilir.");


        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Şifre gereklidir.")
            .MinimumLength(3)
            .WithMessage("Şifre en az 3 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Şifre en fazla 100 karakter olabilir.");

    }
}