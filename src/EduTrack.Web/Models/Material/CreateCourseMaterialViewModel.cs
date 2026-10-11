using EduTrack.Domain.Enums;

namespace EduTrack.Web.Models.Material;

public class CreateCourseMaterialViewModel
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public string? Url { get; set; }
    public IFormFile? File { get; set; }
}
