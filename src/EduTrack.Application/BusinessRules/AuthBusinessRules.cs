using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using Core.Security;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;


namespace EduTrack.Application.BusinessRules;

public class AuthBusinessRules(IUserRepository userRepository) : IBusinessRule
{
    public void CheckUserExists([NotNull] User? user)
    {
        if (user is null)
           throw new BusinessException("Email adresi veya şifre hatalı.");
    }
    public async Task CheckUserExistsByEmail(string email)
    {
        var userExists = await userRepository.AnyAsync(u => u.Email == email);
        if (userExists)
            throw new BusinessException("Bu email ile kayıtlı bir kullanıcı zaten var.");
    }

    public void CheckUserIsActive(User user)
    {
        if (!user.IsActive)
            throw new BusinessException("Hesabınız devre dışı bırakılmış. Lütfen yöneticiyle iletişime geçin.");
    }

    public void CheckVerificationCode(User? user, string code, DateTime utcNow)
    {
        if (user is null)
            throw new BusinessException("Doğrulama kodu geçersiz.");

        if (user.EmailConfirmed)
            return;

        if (string.IsNullOrEmpty(user.EmailVerificationCode)
            || !string.Equals(user.EmailVerificationCode, code?.Trim(), StringComparison.Ordinal))
            throw new BusinessException("Doğrulama kodu geçersiz.");

        if (user.EmailVerificationExpiresAt is null || user.EmailVerificationExpiresAt < utcNow)
            throw new BusinessException("Doğrulama kodunun süresi dolmuş. Yeni bir kod isteyin.");
    }

    public void CheckUserPasswordMatch(User user, string password)
    {
        var passwordCorrect = HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        if (!passwordCorrect)
            throw new BusinessException("Email adresi veya şifre hatalı.");
    }
}
