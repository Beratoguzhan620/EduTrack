using Core.Domain;

namespace EduTrack.Domain.Entities;

public class Course : Entity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User Instructor { get; set; } = default!;
    public virtual ICollection<CourseStudent> Students { get; set; } = default!;
    public virtual ICollection<StreamPost> StreamPosts { get; set; } = default!;
    public virtual ICollection<CourseMaterial> Materials { get; set; } = default!;
}
