namespace EduTrack.Application.DTOs.Course;

public class CourseCreatedResponse
{
    public Guid Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public DateTime CreatedAt { get; set; }
}

