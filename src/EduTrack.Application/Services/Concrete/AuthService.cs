using System.Net;
using System.Security.Cryptography;
using AutoMapper;
using Core.Exceptions;
using Core.Results;
using Core.Security;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Services.Concrete;

public class AuthService(
    IUserRepository userRepository,
    AuthBusinessRules authBusinessRules,
    IMapper mapper,
    IEmailSender emailSender,
    TimeProvider timeProvider) : IAuthService
{
    public static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);

    public const string EmailNotConfirmedMessage = "Giriş yapabilmek için e-posta adresinizi doğrulamanız gerekiyor.";

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await userRepository.GetUserWithRolesByEmailAsync(request.Email);

            authBusinessRules.CheckUserExists(user);
            authBusinessRules.CheckUserPasswordMatch(user!, request.Password);
            authBusinessRules.CheckUserIsActive(user!);

            var response = mapper.Map<LoginResponse>(user);

            if (!user!.EmailConfirmed)
            {
                // The caller uses Data.IsEmailVerified == false to send the user to the verification page.
                return Result<LoginResponse>.Fail(EmailNotConfirmedMessage, response);
            }

            return Result<LoginResponse>.Ok(response);
        }
        catch (BusinessException ex)
        {
            return Result<LoginResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, string verificationUrl)
    {
        try
        {
            await authBusinessRules.CheckUserExistsByEmail(request.Email);

            var result = HashingHelper.CreatePasswordHash(request.Password);

            var user = mapper.Map<User>(request);
            user.PasswordHash = result.Hash;
            user.PasswordSalt = result.Salt;
            user.IsActive = true;
            user.EmailConfirmed = false;
            IssueVerificationCode(user);

            await userRepository.AddAsync(user);

            var sent = await SendVerificationEmailAsync(user, verificationUrl);

            var response = mapper.Map<RegisterResponse>(user);
            response.VerificationEmailSent = sent;
            response.VerificationCode = user.EmailVerificationCode!;

            return Result<RegisterResponse>.Ok(response);
        }
        catch (BusinessException ex)
        {
            return Result<RegisterResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result> VerifyEmailAsync(VerificationRequest request)
    {
        try
        {
            var user = await userRepository.GetAsync(u => u.Email == request.Email);

            authBusinessRules.CheckVerificationCode(user, request.Code, timeProvider.GetUtcNow().UtcDateTime);

            if (user!.EmailConfirmed)
            {
                return Result.Ok("E-posta adresiniz zaten doğrulanmış. Giriş yapabilirsiniz.");
            }

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpiresAt = null;
            await userRepository.UpdateAsync(user);

            return Result.Ok("E-posta adresiniz doğrulandı. Şimdi giriş yapabilirsiniz.");
        }
        catch (BusinessException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<RegisterResponse>> ResendVerificationAsync(string email, string verificationUrl)
    {
        var user = await userRepository.GetAsync(u => u.Email == email);

        // Same answer whether or not the address exists, so the endpoint cannot be used to discover accounts.
        const string genericMessage = "Hesap doğrulanmamışsa yeni bir kod gönderildi.";
        if (user is null || user.EmailConfirmed)
        {
            return Result<RegisterResponse>.Ok(new RegisterResponse { Email = email, VerificationEmailSent = true }, genericMessage);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var issuedAt = user.EmailVerificationExpiresAt?.Subtract(VerificationCodeLifetime);
        if (issuedAt is not null && now - issuedAt < ResendCooldown)
        {
            return Result<RegisterResponse>.Fail("Yeni kod istemeden önce lütfen bir dakika bekleyin.");
        }

        IssueVerificationCode(user);
        await userRepository.UpdateAsync(user);
        var sent = await SendVerificationEmailAsync(user, verificationUrl);

        return Result<RegisterResponse>.Ok(new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            IsActive = user.IsActive,
            VerificationEmailSent = sent,
            VerificationCode = user.EmailVerificationCode!,
        }, genericMessage);
    }

    private void IssueVerificationCode(User user)
    {
        user.EmailVerificationCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        user.EmailVerificationExpiresAt = timeProvider.GetUtcNow().UtcDateTime.Add(VerificationCodeLifetime);
    }

    private Task<bool> SendVerificationEmailAsync(User user, string verificationUrl)
    {
        var link = $"{verificationUrl}?email={Uri.EscapeDataString(user.Email)}&code={user.EmailVerificationCode}";
        var name = WebUtility.HtmlEncode(user.FirstName);

        var body = $"""
            <p>Merhaba {name},</p>
            <p>EduTrack hesabınızı etkinleştirmek için doğrulama kodunuz:</p>
            <p style="font-size:24px;font-weight:600;letter-spacing:4px">{user.EmailVerificationCode}</p>
            <p>Ya da <a href="{link}">bu bağlantıya tıklayın</a>. Kod 24 saat geçerlidir.</p>
            <p>Bu hesabı siz oluşturmadıysanız bu e-postayı yok sayabilirsiniz.</p>
            """;

        return emailSender.SendAsync(user.Email, "EduTrack e-posta doğrulama", body);
    }
}
