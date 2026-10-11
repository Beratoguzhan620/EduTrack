using System;
using System.Collections.Generic;
using System.Text;

namespace EduTrack.Application.DTOs.Auth;

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
}
