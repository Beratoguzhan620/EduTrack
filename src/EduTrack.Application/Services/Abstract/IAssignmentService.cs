using Core.Results;
using EduTrack.Application.DTOs.Assignment;

namespace EduTrack.Application.Services.Abstract;

public interface IAssignmentService
{
    Task<Result<List<AssignmentListItemDto>>> GetCourseAssignmentsAsync(Guid courseId, Guid userId);
    Task<Result<AssignmentDetailDto>> GetAssignmentDetailAsync(Guid assignmentId, Guid userId);
    Task<Result<Guid>> CreateAssignmentAsync(CreateAssignmentRequest request);
    Task<Result> SubmitAsync(SubmitAssignmentRequest request);
    Task<Result<Guid>> GradeAsync(GradeSubmissionRequest request);
}
