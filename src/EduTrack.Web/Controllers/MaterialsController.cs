using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Enums;
using EduTrack.Web.Models.Material;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

[Authorize]
public class MaterialsController(
    ICourseMaterialService courseMaterialService,
    ICourseService courseService,
    IConfiguration configuration) : BaseController
{
    [HttpGet]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> Add(Guid id)
    {
        var course = await courseService.GetCourseDetailAsync(id, GetCurrentUserId());
        if (!course.Success || course.Data is null || !course.Data.IsInstructor)
        {
            TempData["ErrorMessage"] = course.Data is null ? course.Message : "Materyali yalnızca dersin eğitmeni ekleyebilir.";
            return RedirectToAction("Index", "Course");
        }

        return View(new CreateCourseMaterialViewModel { CourseId = id, CourseTitle = course.Data.Title });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> Add(Guid id, CreateCourseMaterialViewModel model)
    {
        model.CourseId = id;

        if (!ModelState.IsValid)
            return await AddAgainAsync(model);

        var result = await courseMaterialService.AddCourseMaterialAsync(new CreateCourseMaterialRequest
        {
            CourseId = id,
            Title = model.Title,
            Description = model.Description,
            Type = model.Type!.Value,
            Url = model.Url?.Trim(),
            File = model.File,
            InstructorId = GetCurrentUserId(),
        });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return await AddAgainAsync(model);
        }

        TempData["SuccessMessage"] = "Materyal eklendi ve ders akışında paylaşıldı.";
        return RedirectToAction("Materials", "Course", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var response = await courseMaterialService.GetCourseMaterialByIdAsync(id, GetCurrentUserId());

        if (!response.Success || response.Data is null)
            return NotFound();

        var material = response.Data;

        if (material.Type == MaterialType.Link && !string.IsNullOrWhiteSpace(material.Url))
            return Redirect(material.Url);

        if (material.Type == MaterialType.Document && !string.IsNullOrWhiteSpace(material.Url))
        {
            var baseFileApiUrl = (configuration["FileApi:BaseUrl"] ?? string.Empty).TrimEnd('/');
            return Redirect($"{baseFileApiUrl}{material.Url}");
        }

        var course = await courseService.GetCourseDetailAsync(material.CourseId, GetCurrentUserId());
        ViewBag.Course = course.Data;

        return View(material);
    }

    private async Task<IActionResult> AddAgainAsync(CreateCourseMaterialViewModel model)
    {
        var course = await courseService.GetCourseDetailAsync(model.CourseId, GetCurrentUserId());
        model.CourseTitle = course.Data?.Title ?? string.Empty;
        return View(model);
    }
}
