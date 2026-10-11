namespace EduTrack.Application.DTOs.Course;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorFullName { get; set; } = string.Empty;
    public bool IsInstructor { get; set; }
    public int StudentCount { get; set; }
    public string? Code { get; set; }
}
