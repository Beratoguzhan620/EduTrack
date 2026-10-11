using Core.BusinessRules;
using Core.Exceptions;
using Core.Security;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;


namespace EduTrack.Application.BusinessRules;

public class AuthBusinessRules(IUserRepository userRepository) : IBusinessRule
{
    public void CheckUserExists(User? user)
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

    public void CheckUserPasswordMatch(User user, string password)
    {
        var passwordCorrect = HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        if (!passwordCorrect)
            throw new BusinessException("Email adresi veya şifre hatalı.");
    }
}
