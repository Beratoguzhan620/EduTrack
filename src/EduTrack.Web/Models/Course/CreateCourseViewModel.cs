using System.ComponentModel.DataAnnotations;

namespace EduTrack.Web.Models.Course;

public class CreateCourseViewModel
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
