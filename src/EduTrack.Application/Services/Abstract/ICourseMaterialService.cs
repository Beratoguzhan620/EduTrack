using Core.Results;
using EduTrack.Application.DTOs.CourseMaterial;

namespace EduTrack.Application.Services.Abstract;

public interface ICourseMaterialService
{
    public Task<Result<List<CourseMaterialDto>>> GetCourseMaterialsByCourseIdAsync(Guid courseId);
    /// <summary>A material of a course the user teaches or attends.</summary>
    public Task<Result<CourseMaterialDto>> GetCourseMaterialByIdAsync(Guid courseMaterialId, Guid userId);
    public Task<Result<CourseMaterialDto>> AddCourseMaterialAsync(CreateCourseMaterialRequest request);
}
