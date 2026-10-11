using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Application.Repositories;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Enums;
using EduTrack.Persistence.Configurations;
using EduTrack.Web.Models.Material;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

public class MaterialsController(
    ICourseMaterialService courseMaterialService,
    IConfiguration configuration
    ) : BaseController
{
    [HttpGet]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public IActionResult Add(Guid id)
    {
        ViewBag.CourseId = id;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> AddAsync(Guid id, CreateCourseMaterialViewModel model)
    {
        ViewBag.CourseId = id;
        if(!ModelState.IsValid)
            return View(model);

        var request = new CreateCourseMaterialRequest
        {
            CourseId = id,
            Title = model.Title,
            Description = model.Description,
            Type = model.Type,
            Url = model.Url,
            File = model.File,
            InstructorId = GetCurrentUserId()
        };

        var result = await courseMaterialService.AddCourseMaterialAsync(request);

        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(model);
        }

        ViewBag.SuccessMessage = "Material başarıyla eklendi.";

        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(Guid id)
    {
        var response = await courseMaterialService.GetCourseMaterialByIdAsync(id);

        if(!response.Success)
            return NotFound();

        var material = response.Data;

        if(material == null)
            return NotFound();

        if (material.Type == MaterialType.Link)
            return Redirect(material.Url);

        if(material.Type == MaterialType.Document)
        {
            var baseFileApiUrl = configuration["FileApi:BaseUrl"].TrimEnd('/');
            var redirectUrl = $"{baseFileApiUrl}{material.Url}";

            return Redirect(redirectUrl);
        }


        return View(response.Data);

    }
}
