using EduTrack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public virtual List<UserRoleDto> UserRoles { get; set; } = default!;
}

public class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}