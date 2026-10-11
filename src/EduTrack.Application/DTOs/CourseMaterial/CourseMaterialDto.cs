using EduTrack.Domain.Enums;

namespace EduTrack.Application.DTOs.CourseMaterial;

public class CourseMaterialDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public string? Url { get; set; }
    public DateTime Date { get; set; }
}