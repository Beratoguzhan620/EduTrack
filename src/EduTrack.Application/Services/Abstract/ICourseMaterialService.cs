using Core.Results;
using EduTrack.Application.DTOs.CourseMaterial;

namespace EduTrack.Application.Services.Abstract;

public interface ICourseMaterialService
{
    public Task<Result<List<CourseMaterialDto>>> GetCourseMaterialsByCourseIdAsync(Guid courseId);
    public Task<Result<CourseMaterialDto>> GetCourseMaterialByIdAsync(Guid courseMaterialId);
    public Task<Result<CourseMaterialDto>> AddCourseMaterialAsync(CreateCourseMaterialRequest request);
}
