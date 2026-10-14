using EduTrack.Application.DTOs.Course;
using EduTrack.Application.DTOs.StreamPost;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Enums;
using EduTrack.Web.Models.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

[Authorize]
public class CourseController(
    ICourseService courseService,
    IStreamPostService streamPostService,
    ICourseMaterialService courseMaterialService,
    IAssignmentService assignmentService) : BaseController
{
    public const int AnnouncementMaxLength = 2000;

    public async Task<IActionResult> Index()
    {
        var result = await courseService.GetUserCoursesAsync(GetCurrentUserId());

        var viewModel = new CourseViewModel { Courses = result.Data ?? [] };

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        var course = await courseService.GetCourseDetailAsync(id, GetCurrentUserId());
        if (!course.Success || course.Data is null)
            return BackToCourses(course.Message);

        var posts = await streamPostService.GetStreamPosts(id);

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(new CourseDetailViewModel
        {
            CourseDetail = course.Data,
            StreamPosts = posts.Data ?? [],
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> MakeAnnouncement(Guid id, string? content)
    {
        var text = content?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            TempData["ErrorMessage"] = "Duyuru metni boş olamaz.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        if (text.Length > AnnouncementMaxLength)
        {
            TempData["ErrorMessage"] = $"Duyuru en fazla {AnnouncementMaxLength} karakter olabilir.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var result = await streamPostService.CreateStreamPost(new CreateStreamPostRequest
        {
            InstructorId = GetCurrentUserId(),
            Content = text,
            CourseId = id,
            Type = StreamPostType.Announcement,
        });

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Duyuru paylaşıldı." : result.Message;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Materials(Guid id)
    {
        var course = await courseService.GetCourseDetailAsync(id, GetCurrentUserId());
        if (!course.Success || course.Data is null)
            return BackToCourses(course.Message);

        var materials = await courseMaterialService.GetCourseMaterialsByCourseIdAsync(id);

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(new CourseDetailViewModel
        {
            CourseDetail = course.Data,
            Materials = materials.Data ?? [],
        });
    }

    [HttpGet]
    public async Task<IActionResult> Assignments(Guid id)
    {
        var course = await courseService.GetCourseDetailAsync(id, GetCurrentUserId());
        if (!course.Success || course.Data is null)
            return BackToCourses(course.Message);

        var assignments = await assignmentService.GetCourseAssignmentsAsync(id, GetCurrentUserId());

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View(new CourseDetailViewModel
        {
            CourseDetail = course.Data,
            Assignments = assignments.Data ?? [],
            NewAssignment = new() { CourseId = id, DueDate = DateTime.Now.Date.AddDays(7).AddHours(23).AddMinutes(59) },
        });
    }

    [HttpGet]
    public async Task<IActionResult> Students(Guid id)
    {
        var course = await courseService.GetCourseDetailAsync(id, GetCurrentUserId());
        if (!course.Success || course.Data is null)
            return BackToCourses(course.Message);

        var students = await courseService.GetCourseStudentsAsync(id, GetCurrentUserId());

        return View(new CourseDetailViewModel
        {
            CourseDetail = course.Data,
            Students = students.Data ?? [],
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> Create(CourseViewModel model)
    {
        var form = model.CreateCourseViewModel;

        if (!ModelState.IsValid || form is null)
            return BackToCourses(FirstError() ?? "Ders bilgilerini kontrol edin.");

        var response = await courseService.CreateCourseAsync(new CreateCourseRequest
        {
            InstructorId = GetCurrentUserId(),
            Title = form.Title.Trim(),
            Description = form.Description.Trim(),
        });

        if (!response.Success || response.Data is null)
            return BackToCourses(response.Message);

        TempData["SuccessMessage"] = $"Ders oluşturuldu. Ders kodu: {response.Data.Code}";
        return RedirectToAction(nameof(Detail), new { id = response.Data.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(CourseViewModel model)
    {
        var form = model.JoinCourseViewModel;

        if (!ModelState.IsValid || form is null)
            return BackToCourses(FirstError() ?? "Ders kodunu kontrol edin.");

        var response = await courseService.JoinCourseAsync(new JoinCourseRequest
        {
            Code = form.Code,
            StudentId = GetCurrentUserId(),
        });

        TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] = response.Message;
        return RedirectToAction(nameof(Index));
    }

    private RedirectToActionResult BackToCourses(string? message)
    {
        TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(message) ? "Ders bulunamadı." : message;
        return RedirectToAction(nameof(Index));
    }

    private string? FirstError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
}
