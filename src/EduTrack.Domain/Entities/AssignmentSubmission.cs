using Core.Domain;

namespace EduTrack.Domain.Entities;

public class AssignmentSubmission : Entity<Guid>
{
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SubmittedAt { get; set; }

    public int? Grade { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = default!;
    public virtual User Student { get; set; } = default!;

    public bool IsGraded => Grade is not null;
}
