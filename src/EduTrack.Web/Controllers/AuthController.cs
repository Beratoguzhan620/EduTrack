using Core.Results;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Application.Services.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduTrack.Web.Controllers;


public class AuthController(IAuthService authService, IWebHostEnvironment environment) : BaseController
{
    private string VerificationUrl => Url.Action(nameof(ConfirmEmail), "Auth", null, Request.Scheme)!;

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


        var result = await authService.RegisterAsync(model, VerificationUrl);

        if (!result.Success || result.Data is null)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = "Hesabınız oluşturuldu. E-postanıza gönderilen 6 haneli kodu girin.";
        ShowDevelopmentCode(result.Data);
        return RedirectToAction(nameof(VerifyEmail), new { email = result.Data.Email });
    }


    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        return View();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);


        var result = await authService.LoginAsync(model);

        if (result.Data is { IsEmailVerified: false })
        {
            TempData["InfoMessage"] = result.Message;
            return RedirectToAction(nameof(VerifyEmail), new { email = model.Email });
        }

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



    [HttpGet("VerifyEmail")]
    public IActionResult VerifyEmail(string? email)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.InfoMessage = TempData["InfoMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        ViewBag.DevelopmentCode = TempData["DevelopmentCode"];
        return View(new VerificationRequest { Email = email ?? string.Empty });
    }

    [HttpPost("VerifyEmail")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerificationRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await authService.VerifyEmailAsync(model);
        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    /// <summary>Target of the link inside the verification e-mail.</summary>
    [HttpGet("VerifyEmail/Confirm")]
    public async Task<IActionResult> ConfirmEmail(string email, string code)
    {
        var result = await authService.VerifyEmailAsync(new VerificationRequest { Email = email, Code = code });
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(VerifyEmail), new { email });
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost("VerifyEmail/Resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendVerification(string email)
    {
        var result = await authService.ResendVerificationAsync(email, VerificationUrl);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        if (result.Data is not null)
            ShowDevelopmentCode(result.Data);

        return RedirectToAction(nameof(VerifyEmail), new { email });
    }

    /// <summary>
    /// Without SMTP settings no e-mail goes out; in Development the code is shown on the page
    /// so the flow can be completed locally. Never shown in other environments.
    /// </summary>
    private void ShowDevelopmentCode(RegisterResponse response)
    {
        if (environment.IsDevelopment() && !response.VerificationEmailSent && !string.IsNullOrEmpty(response.VerificationCode))
            TempData["DevelopmentCode"] = response.VerificationCode;
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
