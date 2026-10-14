using Core.Exceptions;
using Core.Security;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Moq;

namespace EduTrack.Application.UnitTest.BusinessRules;

public class AuthBusinessRulesTests
{
    private readonly AuthBusinessRules _rules = new(Mock.Of<IUserRepository>());
    private static readonly DateTime Now = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    private static User Pending(string code = "123456", int expiresInMinutes = 30) => new()
    {
        EmailVerificationCode = code,
        EmailVerificationExpiresAt = Now.AddMinutes(expiresInMinutes),
    };

    [Fact]
    public void Inactive_user_cannot_sign_in()
    {
        _rules.CheckUserIsActive(new User { IsActive = true });
        Assert.Throws<BusinessException>(() => _rules.CheckUserIsActive(new User { IsActive = false }));
    }

    [Fact]
    public void Correct_code_is_accepted_with_surrounding_spaces()
    {
        _rules.CheckVerificationCode(Pending(), " 123456 ", Now);
    }

    [Theory]
    [InlineData("654321")]
    [InlineData("")]
    public void Wrong_code_is_rejected(string code)
    {
        Assert.Throws<BusinessException>(() => _rules.CheckVerificationCode(Pending(), code, Now));
    }

    [Fact]
    public void Expired_code_is_rejected_with_its_own_message()
    {
        var ex = Assert.Throws<BusinessException>(() => _rules.CheckVerificationCode(Pending(expiresInMinutes: -1), "123456", Now));
        Assert.Contains("süresi", ex.Message);
    }

    [Fact]
    public void Unknown_user_is_rejected_and_confirmed_user_passes()
    {
        Assert.Throws<BusinessException>(() => _rules.CheckVerificationCode(null, "123456", Now));
        _rules.CheckVerificationCode(new User { EmailConfirmed = true }, "anything", Now);
    }

    [Fact]
    public void Password_must_match_the_stored_hash()
    {
        var hash = HashingHelper.CreatePasswordHash("Demo123!");
        var user = new User { PasswordHash = hash.Hash, PasswordSalt = hash.Salt };

        _rules.CheckUserPasswordMatch(user, "Demo123!");
        Assert.Throws<BusinessException>(() => _rules.CheckUserPasswordMatch(user, "demo123!"));
    }
}
