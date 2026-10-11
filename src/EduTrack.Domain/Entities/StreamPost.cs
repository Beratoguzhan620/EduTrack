using Core.Domain;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class StreamPost : Entity<Guid>
{
    public Guid CourseId { get; set; }
    public Guid InstructorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public StreamPostType Type { get; set; }
    public virtual Course Course { get; set; } = default!;
    public virtual User Instructor { get; set; } = default!;
}
