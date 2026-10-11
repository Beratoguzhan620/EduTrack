using Core.DTOs;
using Core.Results;
using EduTrack.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.Services.Abstract;

public interface IUserService
{
    Task<Result<Paged<UserDto>>> GetAllUsersAsync(GetAllUsersQuery request);
    Task<Result<UserDto>> GetUserByIdAsync(Guid id);
    Task<Result<List<RoleDto>>> GetAllRolesAsync();
    Task<Result> UpdateUserAsync(UpdateUserRequest request);
}
