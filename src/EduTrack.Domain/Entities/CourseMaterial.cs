using Core.Domain;
using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

public class CourseMaterial : Entity<Guid>
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public string? Url{ get; set; }
    public DateTime Date { get; set; }
    public virtual Course Course { get; set; } = default!;
}
