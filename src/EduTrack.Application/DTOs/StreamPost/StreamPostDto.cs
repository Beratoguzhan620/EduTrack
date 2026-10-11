using EduTrack.Domain.Enums;

namespace EduTrack.Application.DTOs.StreamPost;

public class StreamPostDto
{
    public Guid CourseId { get; set; }
    public string InstructorInitials { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public StreamPostType Type { get; set; }
    public DateTime Date { get; set; }
}
