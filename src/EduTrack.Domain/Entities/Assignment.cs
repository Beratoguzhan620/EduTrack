using Core.Domain;

namespace EduTrack.Domain.Entities;

public class Assignment : Entity<Guid>
{
    public Guid CourseId { get; set; }
    public Guid CreatedById { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int MaxPoints { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = default!;
    public virtual User CreatedBy { get; set; } = default!;
    public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = [];
}
