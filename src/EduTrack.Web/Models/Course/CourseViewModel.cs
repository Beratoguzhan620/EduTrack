using EduTrack.Application.DTOs.Course;

namespace EduTrack.Web.Models.Course;

public class CourseViewModel
{
    public List<CourseDto> Courses { get; set; } = new();
    public CreateCourseViewModel? CreateCourseViewModel { get; set; }
    public JoinCourseViewModel? JoinCourseViewModel { get; set; }
}
