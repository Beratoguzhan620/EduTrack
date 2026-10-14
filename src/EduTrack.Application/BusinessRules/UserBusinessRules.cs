using System.Diagnostics.CodeAnalysis;
using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.BusinessRules;

public class UserBusinessRules(IUserRepository userRepository) : IBusinessRule
{
    public void CheckUserExists([NotNull] User? user)
    {
        if (user is null)
            throw new BusinessException("Kullanıcı bulunamadı.");
    }

    public async Task CheckEmailIsUniqueForUserAsync(Guid userId, string email)
    {
        var emailExists = await userRepository.AnyAsync(x => x.Id != userId && x.Email == email);
        if (emailExists)
            throw new BusinessException("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
    }

    public void CheckRolesAreValid(List<int> validRoleIds, IEnumerable<int> requestedRoleIds)
    {
        if (validRoleIds.Count != requestedRoleIds.Distinct().Count())
            throw new BusinessException("Seçilen roller geçersiz.");
    }
}
