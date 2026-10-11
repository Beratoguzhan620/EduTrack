using EduTrack.Application.Services.Abstract;
using EduTrack.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EduTrack.Web.Controllers;

[Authorize]
public class HomeController(ICourseService courseService) : BaseController
{

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Course");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
