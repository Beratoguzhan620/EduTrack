using System.ComponentModel.DataAnnotations;

namespace EduTrack.Web.Models.Users;

public class EditUserViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    public List<int> SelectedRoleIds { get; set; } = [];
    public List<UserRoleOptionViewModel> Roles { get; set; } = [];
}

public class UserRoleOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
