using Core.Results;
using EduTrack.Application.DTOs.Auth;

namespace EduTrack.Application.Services.Abstract;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}
