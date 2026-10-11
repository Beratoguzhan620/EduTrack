using EduTrack.Domain.Enums;

namespace EduTrack.Application.DTOs.StreamPost;

public class CreateStreamPostRequest
{
    public Guid CourseId { get; set; }
    public Guid InstructorId { get; set; }
    public StreamPostType Type { get; set; }
    public string Content { get; set; } = string.Empty;
}
