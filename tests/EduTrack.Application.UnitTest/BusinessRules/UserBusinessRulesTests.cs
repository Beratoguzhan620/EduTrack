using Core.Exceptions;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.Repositories;
using Moq;

namespace EduTrack.Application.UnitTest.BusinessRules;

public class UserBusinessRulesTests
{
    private readonly UserBusinessRules _rules = new(Mock.Of<IUserRepository>());

    [Fact]
    public void Requested_roles_must_all_exist()
    {
        _rules.CheckRolesAreValid([1, 2], [1, 2]);
        _rules.CheckRolesAreValid([2], [2, 2]);

        Assert.Throws<BusinessException>(() => _rules.CheckRolesAreValid([1], [1, 99]));
    }

    [Fact]
    public void Missing_user_is_reported_in_turkish()
    {
        var ex = Assert.Throws<BusinessException>(() => _rules.CheckUserExists(null));
        Assert.Equal("Kullanıcı bulunamadı.", ex.Message);
    }
}
