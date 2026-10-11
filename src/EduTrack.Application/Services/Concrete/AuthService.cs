using AutoMapper;
using Core.Exceptions;
using Core.Results;
using Core.Security;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;


namespace BlogApp.Application.Services.Concrete;

public class AuthService(
    IUserRepository userRepository, 
    AuthBusinessRules authBusinessRules,
    IMapper mapper) : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await userRepository.GetUserWithRolesByEmailAsync(request.Email);

            authBusinessRules.CheckUserExists(user);
            authBusinessRules.CheckUserPasswordMatch(user!, request.Password);

            var response = mapper.Map<LoginResponse>(user);


            return Result<LoginResponse>.Ok(response);
        }
        catch (BusinessException ex)
        {
            return Result<LoginResponse>.Fail(ex.Message);
        }

    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            await authBusinessRules.CheckUserExistsByEmail(request.Email);

            var result = HashingHelper.CreatePasswordHash(request.Password);

            var user = mapper.Map<User>(request);
            user.PasswordHash = result.Hash;
            user.PasswordSalt = result.Salt;
            user.IsActive = true; //TODO: email doğrulama eklendiğinde burayı false yapalım

            await userRepository.AddAsync(user);

            var response = mapper.Map<RegisterResponse>(user);

            return Result<RegisterResponse>.Ok(response);

        }
        catch (BusinessException ex)
        {
            return Result<RegisterResponse>.Fail(ex.Message);
        }
    }

}
