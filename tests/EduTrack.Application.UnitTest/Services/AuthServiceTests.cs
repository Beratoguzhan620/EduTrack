using AutoMapper;
using BlogApp.Application.Services.Concrete;
using Core.Security;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;
using Moq;

namespace EduTrack.Application.UnitTest.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _authBusinessRules = new AuthBusinessRules(_userRepositoryMock.Object);
        _mapperMock = new Mock<IMapper>();
        _sut = new AuthService(_userRepositoryMock.Object, _authBusinessRules, _mapperMock.Object);
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
        };
        var loginResponse = new LoginResponse();
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
        var result = await _sut.RegisterAsync(request);

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
        var result = await _sut.RegisterAsync(request);

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
        await _sut.RegisterAsync(request);

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
        await _sut.RegisterAsync(request);

        // Assert
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }
}
