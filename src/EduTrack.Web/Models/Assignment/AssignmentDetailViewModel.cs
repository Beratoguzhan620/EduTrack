using EduTrack.Application.DTOs.Assignment;
using EduTrack.Application.DTOs.Course;

namespace EduTrack.Web.Models.Assignment;

public class AssignmentDetailViewModel
{
    public CourseDto Course { get; set; } = new();
    public AssignmentDetailDto Assignment { get; set; } = new();
    public SubmitAssignmentRequest Submission { get; set; } = new();
}
