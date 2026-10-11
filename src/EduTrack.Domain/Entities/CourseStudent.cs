using Core.Domain;

namespace EduTrack.Domain.Entities;

public class CourseStudent : Entity<Guid>
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EnrolledAt { get; set; }

    public virtual Course Course { get; set; } = default!;
    public virtual User User { get; set; } = default!;
}