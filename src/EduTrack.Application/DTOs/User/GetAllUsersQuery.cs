using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.User;

public class GetAllUsersQuery : PagedRequest
{
    public string? Search { get; set; } = string.Empty;
    public int? RoleId { get; set; } 
}
