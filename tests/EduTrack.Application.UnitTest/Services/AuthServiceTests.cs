using AutoMapper;
using Core.Security;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Application.Services.Concrete;
using Microsoft.Extensions.Time.Testing;
using EduTrack.Domain.Entities;
using Moq;

namespace EduTrack.Application.UnitTest.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly FakeTimeProvider _time;
    private readonly AuthService _sut;

    private const string VerificationUrl = "https://localhost/VerifyEmail/Confirm";

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _authBusinessRules = new AuthBusinessRules(_userRepositoryMock.Object);
        _mapperMock = new Mock<IMapper>();
        _emailSenderMock = new Mock<IEmailSender>();
        _emailSenderMock
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));
        _sut = new AuthService(_userRepositoryMock.Object, _authBusinessRules, _mapperMock.Object, _emailSenderMock.Object, _time);
    }

    // LoginAsync tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsOkWithLoginResponse()
    {
        // Arrange
        const string password = "TestPassword123";
        var hashResult = HashingHelper.CreatePasswordHash(password);
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashResult.Hash,
            PasswordSalt = hashResult.Salt,
            IsActive = true,
            EmailConfirmed = true,
        };
        var loginResponse = new LoginResponse { IsEmailVerified = true };
        var request = new LoginRequest { Email = "test@example.com", Password = password };

        _userRepositoryMock
            .Setup(r => r.GetUserWithRolesByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _mapperMock
            .Setup(m => m.Map<LoginResponse>(user))
            .Returns(loginResponse);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(loginResponse, result.Data);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailResult()
    {
        // Arrange
        var request = new LoginRequest { Email = "notfound@example.com", Password = "any" };

        _userRepositoryMock
            .Setup(r => r.GetUserWithRolesByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailResult()
    {
        // Arrange
        var hashResult = HashingHelper.CreatePasswordHash("CorrectPassword");
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashResult.Hash,
            PasswordSalt = hashResult.Salt,
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _userRepositoryMock
            .Setup(r => r.GetUserWithRolesByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    // RegisterAsync tests

    [Fact]
    public async Task RegisterAsync_NewEmail_ReturnsOkWithRegisterResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "new@example.com",
            Password = "Password123",
        };
        var user = new User { Email = request.Email };
        var registerResponse = new RegisterResponse();

        _userRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);
        _mapperMock
            .Setup(m => m.Map<User>(request))
            .Returns(user);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(user);
        _mapperMock
            .Setup(m => m.Map<RegisterResponse>(It.IsAny<User>()))
            .Returns(registerResponse);

        // Act
        var result = await _sut.RegisterAsync(request, VerificationUrl);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(registerResponse, result.Data);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsFailResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "existing@example.com",
            Password = "Password123",
        };

        _userRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RegisterAsync(request, VerificationUrl);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_SetsPasswordHashAndSaltOnUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "new@example.com",
            Password = "Password123",
        };
        var user = new User { Email = request.Email };
        User? capturedUser = null;

        _userRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);
        _mapperMock
            .Setup(m => m.Map<User>(request))
            .Returns(user);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync(user);
        _mapperMock
            .Setup(m => m.Map<RegisterResponse>(It.IsAny<User>()))
            .Returns(new RegisterResponse());

        // Act
        await _sut.RegisterAsync(request, VerificationUrl);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotEmpty(capturedUser!.PasswordHash);
        Assert.NotEmpty(capturedUser.PasswordSalt);
        Assert.True(capturedUser.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CallsAddAsync()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "new@example.com",
            Password = "Password123",
        };
        var user = new User { Email = request.Email };

        _userRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<User>(request)).Returns(user);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<RegisterResponse>(It.IsAny<User>())).Returns(new RegisterResponse());

        // Act
        await _sut.RegisterAsync(request, VerificationUrl);

        // Assert
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUnconfirmedUserAndSendsCode()
    {
        var request = new RegisterRequest { FirstName = "Ece", LastName = "Kaya", Email = "ece@example.com", Password = "Password123" };
        var user = new User { Email = request.Email, FirstName = request.FirstName };
        User? captured = null;

        _userRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<User>(request)).Returns(user);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<RegisterResponse>(It.IsAny<User>())).Returns(new RegisterResponse());

        var result = await _sut.RegisterAsync(request, VerificationUrl);

        Assert.True(result.Success);
        Assert.False(captured!.EmailConfirmed);
        Assert.Matches(@"^\d{6}$", captured.EmailVerificationCode);
        Assert.Equal(_time.GetUtcNow().UtcDateTime.AddHours(24), captured.EmailVerificationExpiresAt);
        Assert.True(result.Data!.VerificationEmailSent);
        _emailSenderMock.Verify(e => e.SendAsync(
            "ece@example.com",
            It.IsAny<string>(),
            It.Is<string>(body => body.Contains(captured.EmailVerificationCode!) && body.Contains(VerificationUrl)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UnconfirmedEmail_FailsButReturnsDataForRedirect()
    {
        var hash = HashingHelper.CreatePasswordHash("Password123");
        var user = new User { Email = "a@example.com", PasswordHash = hash.Hash, PasswordSalt = hash.Salt, IsActive = true, EmailConfirmed = false };
        _userRepositoryMock.Setup(r => r.GetUserWithRolesByEmailAsync(user.Email)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<LoginResponse>(user)).Returns(new LoginResponse { IsEmailVerified = false });

        var result = await _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "Password123" });

        Assert.False(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data!.IsEmailVerified);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsFailWithoutData()
    {
        var hash = HashingHelper.CreatePasswordHash("Password123");
        var user = new User { Email = "a@example.com", PasswordHash = hash.Hash, PasswordSalt = hash.Salt, IsActive = false, EmailConfirmed = true };
        _userRepositoryMock.Setup(r => r.GetUserWithRolesByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "Password123" });

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task VerifyEmailAsync_CorrectCode_ConfirmsAndClearsCode()
    {
        var user = UnconfirmedUser("123456", _time.GetUtcNow().UtcDateTime.AddHours(1));
        SetupGetUser(user);

        var result = await _sut.VerifyEmailAsync(new VerificationRequest { Email = user.Email, Code = "123456" });

        Assert.True(result.Success);
        Assert.True(user.EmailConfirmed);
        Assert.Null(user.EmailVerificationCode);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WrongCode_Fails()
    {
        var user = UnconfirmedUser("123456", _time.GetUtcNow().UtcDateTime.AddHours(1));
        SetupGetUser(user);

        var result = await _sut.VerifyEmailAsync(new VerificationRequest { Email = user.Email, Code = "654321" });

        Assert.False(result.Success);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public async Task VerifyEmailAsync_ExpiredCode_Fails()
    {
        var user = UnconfirmedUser("123456", _time.GetUtcNow().UtcDateTime.AddMinutes(-1));
        SetupGetUser(user);

        var result = await _sut.VerifyEmailAsync(new VerificationRequest { Email = user.Email, Code = "123456" });

        Assert.False(result.Success);
        Assert.Contains("süresi", result.Message);
    }

    [Fact]
    public async Task ResendVerificationAsync_WithinCooldown_Fails()
    {
        // Code issued 30 seconds ago.
        var issuedAt = _time.GetUtcNow().UtcDateTime.AddSeconds(-30);
        var user = UnconfirmedUser("123456", issuedAt.Add(AuthService.VerificationCodeLifetime));
        SetupGetUser(user);

        var result = await _sut.ResendVerificationAsync(user.Email, VerificationUrl);

        Assert.False(result.Success);
        _emailSenderMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResendVerificationAsync_AfterCooldown_IssuesNewCode()
    {
        var issuedAt = _time.GetUtcNow().UtcDateTime.AddMinutes(-5);
        var user = UnconfirmedUser("123456", issuedAt.Add(AuthService.VerificationCodeLifetime));
        SetupGetUser(user);

        var result = await _sut.ResendVerificationAsync(user.Email, VerificationUrl);

        Assert.True(result.Success);
        Assert.Equal(_time.GetUtcNow().UtcDateTime.AddHours(24), user.EmailVerificationExpiresAt);
        _emailSenderMock.Verify(e => e.SendAsync(user.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_UnknownEmail_ReturnsGenericSuccess()
    {
        _userRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ResendVerificationAsync("nobody@example.com", VerificationUrl);

        Assert.True(result.Success);
        _emailSenderMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static User UnconfirmedUser(string code, DateTime expiresAt) => new()
    {
        Email = "student@example.com",
        FirstName = "Ali",
        IsActive = true,
        EmailConfirmed = false,
        EmailVerificationCode = code,
        EmailVerificationExpiresAt = expiresAt,
    };

    private void SetupGetUser(User user) =>
        _userRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
}
