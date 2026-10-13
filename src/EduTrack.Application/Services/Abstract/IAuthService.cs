using Core.Results;
using EduTrack.Application.DTOs.Auth;

namespace EduTrack.Application.Services.Abstract;

public interface IAuthService
{
    /// <param name="verificationUrl">Absolute URL of the e-mail confirmation endpoint; email and code are appended.</param>
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, string verificationUrl);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> VerifyEmailAsync(VerificationRequest request);
    Task<Result<RegisterResponse>> ResendVerificationAsync(string email, string verificationUrl);
}
