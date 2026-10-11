using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EduTrack.Application.DTOs.CourseMaterial;

public class CreateCourseMaterialRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public string? Url { get; set; }
    public IFormFile? File { get; set; }
    public Guid InstructorId { get; set; }

}