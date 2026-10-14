using System.ComponentModel.DataAnnotations;

namespace EduTrack.Web.Models.Course;

public class JoinCourseViewModel
{
    [Display(Name = "Ders kodu")]
    [Required(ErrorMessage = "Ders kodu gereklidir.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Ders kodu 8 karakterdir.")]
    public string Code { get; set; } = string.Empty;
}
