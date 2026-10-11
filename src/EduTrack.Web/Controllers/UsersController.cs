using EduTrack.Application.Services.Abstract;
using EduTrack.Application.DTOs.User;
using EduTrack.Domain.Constants;
using EduTrack.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
public class UsersController(IUserService userService) : BaseController
{
    public async Task<IActionResult> Index(GetUsersViewModel model)
    {
        var response = await userService.GetAllUsersAsync(model.Query);
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        model.Response = response.Data;

        return View(model);
    }

    public async Task<IActionResult> AddRole(Guid userId, int? roleId = null)
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userResponse = await userService.GetUserByIdAsync(id);

        if (!userResponse.Success || userResponse.Data is null)
        {
            return NotFound();
        }

        var model = await CreateEditUserViewModelAsync(userResponse.Data);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await GetRoleOptionsAsync();
            return View(model);
        }

        var response = await userService.UpdateUserAsync(new UpdateUserRequest
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            IsActive = model.IsActive,
            RoleIds = model.SelectedRoleIds
        });

        if (!response.Success)
        {
            ViewBag.ErrorMessage = response.Message;
            model.Roles = await GetRoleOptionsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task<EditUserViewModel> CreateEditUserViewModelAsync(UserDto user)
    {
        return new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            SelectedRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList(),
            Roles = await GetRoleOptionsAsync()
        };
    }

    private async Task<List<UserRoleOptionViewModel>> GetRoleOptionsAsync()
    {
        var rolesResponse = await userService.GetAllRolesAsync();

        return rolesResponse.Data?
            .Select(role => new UserRoleOptionViewModel
            {
                Id = role.Id,
                Name = role.Name
            })
            .ToList() ?? [];
    }

}
