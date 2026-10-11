using Core.Results;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Services.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduTrack.Web.Controllers;


public class AuthController(IAuthService authService) : BaseController
{
    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);


        var result = await authService.RegisterAsync(model);

        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(model);
        }

        return RedirectToAction(nameof(Login));
    }


    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);


        var result = await authService.LoginAsync(model);

        if (!result.Success || result.Data is null)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(model);
        }

        await CookieAuthSignIn(result, model.RememberMe);


        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);


        return RedirectToAction("Index", "Home");
    }



    private async Task CookieAuthSignIn(Result<LoginResponse> result, bool rememberMe)
    {
        var claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.NameIdentifier, result.Data!.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, result.Data.FirstName));
        claims.Add(new Claim(ClaimTypes.Surname, result.Data.LastName));
        claims.Add(new Claim(ClaimTypes.Email, result.Data.Email));
        claims.Add(new Claim("FullName", $"{result.Data.FirstName} {result.Data.LastName}"));
        claims.Add(new Claim("IsActive", result.Data.IsActive.ToString()));

        foreach (var role in result.Data.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTime.UtcNow.AddHours(3),
            AllowRefresh = true,
        };

        await HttpContext.SignInAsync(principal, authProperties);
    }


    [HttpGet("Logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}
