using Core.BusinessRules;
using Core.Exceptions;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.BusinessRules;

public class UserBusinessRules(IUserRepository userRepository) : IBusinessRule
{
    public void CheckUserExists(User? user)
    {
        if (user is null)
            throw new BusinessException("User not found.");
    }

    public async Task CheckEmailIsUniqueForUserAsync(Guid userId, string email)
    {
        var emailExists = await userRepository.AnyAsync(x => x.Id != userId && x.Email == email);
        if (emailExists)
            throw new BusinessException("Email is already used by another user.");
    }

    public void CheckRolesAreValid(List<int> validRoleIds, IEnumerable<int> requestedRoleIds)
    {
        if (validRoleIds.Count != requestedRoleIds.Distinct().Count())
            throw new BusinessException("Selected roles are invalid.");
    }
}
