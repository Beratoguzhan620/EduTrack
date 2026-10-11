using Core.Results;
using Core.Exceptions;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.User;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EduTrack.Application.Repositories;
using Core.DTOs;

namespace EduTrack.Application.Services.Concrete;

public class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    UserBusinessRules userBusinessRules) : IUserService
{
    public async Task<Result<Paged<UserDto>>> GetAllUsersAsync(GetAllUsersQuery request)
    {
        //TODO: dynamic query
        var search = request.Search?.Trim();

        var users = await userRepository.GetPagedAsync(
            predicate: u =>
                (string.IsNullOrEmpty(search) ||
                    (u.FirstName != null && u.FirstName.Contains(search)) ||
                    (u.LastName != null && u.LastName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)))
                &&
                (request.RoleId == null ||
                    u.UserRoles.Any(ur => ur.RoleId == request.RoleId)),

            index: request.Index, 
            size: request.Size, 
            include: x => x.Include(u => u.UserRoles).ThenInclude(ur => ur.Role));


        var response = new Paged<UserDto>()
        {
            Items = users.Items.Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                IsActive = u.IsActive,
                UserRoles = u.UserRoles.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name
                }).ToList()
            }).ToList(),
            Count = users.Count,
            From = users.From,
            Index = users.Index,
            Size = users.Size,
            Pages = users.Pages,
        };

        return Result<Paged<UserDto>>.Ok(response);
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await userRepository.GetAsync(
                predicate: x => x.Id == id,
                include: x => x.Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

            userBusinessRules.CheckUserExists(user);

            return Result<UserDto>.Ok(new UserDto
            {
                Id = user!.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                UserRoles = user.UserRoles.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name
                }).ToList()
            });
        }
        catch (BusinessException ex)
        {
            return Result<UserDto>.Fail(ex.Message);
        }
    }

    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        var roles = await roleRepository.GetListAsync(orderBy: r => r.OrderBy(r => r.Name));

        var result = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name
        })
        .ToList();

        return Result<List<RoleDto>>.Ok(result);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserRequest request)
    {
        try
        {
            var user = await userRepository.GetAsync(
                x => x.Id == request.Id,
                include: x => x.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                );



            userBusinessRules.CheckUserExists(user);
            await userBusinessRules.CheckEmailIsUniqueForUserAsync(request.Id, request.Email);

            var validRoleIds = await roleRepository.Query()
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            userBusinessRules.CheckRolesAreValid(validRoleIds, request.RoleIds);

            user!.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = request.Email.Trim();
            user.IsActive = request.IsActive;

            var selectedRoleIds = validRoleIds.ToHashSet();
            var rolesToRemove = user.UserRoles
                .Where(ur => !selectedRoleIds.Contains(ur.RoleId))
                .ToList();
            var existingRoleIds = user.UserRoles
                .Select(ur => ur.RoleId)
                .ToHashSet();
            var rolesToAdd = selectedRoleIds
                .Where(roleId => !existingRoleIds.Contains(roleId))
                .Select(roleId => new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                })
                .ToList();

            await userRoleRepository.DeleteRangeAsync(rolesToRemove);
            await userRoleRepository.AddRangeAsync(rolesToAdd);

            return Result.Ok("User updated successfully.");
        }
        catch (BusinessException ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}

