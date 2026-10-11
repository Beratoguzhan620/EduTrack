using System.ComponentModel.DataAnnotations;

namespace EduTrack.Web.Models.Course;

public class JoinCourseViewModel
{
    [Required, MinLength(8), MaxLength(8)]
    public string Code { get; set; }
}
