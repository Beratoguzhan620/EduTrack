using Core.Results;
using EduTrack.Application.DTOs.Course;
using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Application.DTOs.StreamPost;
using EduTrack.Application.Services.Abstract;
using EduTrack.Domain.Constants;
using EduTrack.Domain.Enums;
using EduTrack.Persistence.Configurations;
using EduTrack.Web.Models.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Web.Controllers;

public class CourseController(
    ICourseService courseService,
    IStreamPostService streamPostService,
    ICourseMaterialService courseMaterialService) : BaseController
{

    public async Task<IActionResult> Index()
    {
        Result<List<CourseDto>> result;

        var userId = GetCurrentUserId();

        result = await courseService.GetUserCoursesAsync(userId);

        var viewModel = new CourseViewModel();
        viewModel.Courses = result.Data;

        return View(viewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Detail(Guid id)
    {
        var courseDetailResult = await GetCourseDetailAsync(id);
        CourseDetailViewModel viewModel = new();

        viewModel.CourseDetail = courseDetailResult;
        var streamPostResult = await streamPostService.GetStreamPosts(id);
        viewModel.StreamPosts = streamPostResult.Data;
        return View(viewModel);

    }




    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> MakeAnnouncement(Guid id, string content)
    {
        var dto = new CreateStreamPostRequest
        {
            InstructorId = GetCurrentUserId(),
            Content = content,
            CourseId = id,
            Type = StreamPostType.Announcement
        };

        await streamPostService.CreateStreamPost(dto);

        return RedirectToAction(nameof(Detail), new { id });
    }


    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Materials(Guid id)
    {
        var courseDetailResult = await GetCourseDetailAsync(id);
        var courseMaterialResult = await courseMaterialService.GetCourseMaterialsByCourseIdAsync(id);
        var viewModel = new CourseDetailViewModel();
        if (courseMaterialResult.Success)
        {
            viewModel.CourseDetail = courseDetailResult;
            viewModel.Materials = courseMaterialResult.Data;
        }
        return View(viewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AssignmentsAsync(Guid id)
    {
        var courseDetailResult = await GetCourseDetailAsync(id);
        var model = new CourseDetailViewModel();
        model.CourseDetail = courseDetailResult;
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> StudentsAsync(Guid id)
    {
        var courseDetailResult = await GetCourseDetailAsync(id);
        var model = new CourseDetailViewModel();
        model.CourseDetail = courseDetailResult;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Instructor}")]
    public async Task<IActionResult> Create(CourseViewModel model)
    {

        if (!ModelState.IsValid)
            return RedirectToAction("Index", "Home");


        var request = new CreateCourseRequest
        {
            InstructorId = GetCurrentUserId(),
            Title = model.CreateCourseViewModel.Title,
            Description = model.CreateCourseViewModel.Description
        };

        var response = await courseService.CreateCourseAsync(request);

        if (!response.Success)
        {
            ViewBag.ErrorMessage = response.Message;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Join(CourseViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var request = new JoinCourseRequest
        {
            Code = model.JoinCourseViewModel.Code,
            StudentId = GetCurrentUserId()
        };

        var response = await courseService.JoinCourseAsync(request);

        if (!response.Success)
        {
            ViewBag.ErrorMessage = response.Message;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }


    private async Task<CourseDto> GetCourseDetailAsync(Guid courseId)
    {
        var userId = GetCurrentUserId();
        var courseDetailResult = await courseService.GetCourseDetailAsync(courseId, userId);

        if (courseDetailResult.Success && courseDetailResult.Data != null)
        {
            return courseDetailResult.Data;
        }

        return new CourseDto();
    }

}
