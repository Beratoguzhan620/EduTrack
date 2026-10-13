using EduTrack.Application.DTOs.Assignment;
using EduTrack.Application.Services.Abstract;
using EduTrack.Web.Models.Assignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

[Authorize]
[Route("Assignments")]
public class AssignmentsController(IAssignmentService assignmentService, ICourseService courseService) : BaseController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await assignmentService.GetAssignmentDetailAsync(id, userId);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Index", "Course");
        }

        var course = await courseService.GetCourseDetailAsync(result.Data.CourseId, userId);

        var model = new AssignmentDetailViewModel
        {
            Course = course.Data ?? new(),
            Assignment = result.Data,
            Submission = new SubmitAssignmentRequest
            {
                AssignmentId = id,
                Text = result.Data.MySubmission?.Text ?? string.Empty,
            },
        };

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "NewAssignment")] CreateAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = FirstError() ?? "Ödev bilgilerini kontrol edin.";
            return RedirectToAction("Assignments", "Course", new { id = request.CourseId });
        }

        request.InstructorId = GetCurrentUserId();
        // The form posts local time (datetime-local); the database stores UTC.
        request.DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Local).ToUniversalTime();

        var result = await assignmentService.CreateAssignmentAsync(request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Assignments", "Course", new { id = request.CourseId });
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id = result.Data });
    }

    [HttpPost("{id:guid}/Submit")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Submit(Guid id, [Bind(Prefix = "Submission")] SubmitAssignmentRequest request)
    {
        request.AssignmentId = id;
        request.StudentId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = FirstError() ?? "Teslim bilgilerini kontrol edin.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var result = await assignmentService.SubmitAsync(request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("Grade")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(GradeSubmissionRequest request, Guid assignmentId)
    {
        request.InstructorId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = FirstError() ?? "Not bilgilerini kontrol edin.";
            return RedirectToAction(nameof(Detail), new { id = assignmentId });
        }

        var result = await assignmentService.GradeAsync(request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Detail), new { id = assignmentId });
    }

    private string? FirstError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
}
