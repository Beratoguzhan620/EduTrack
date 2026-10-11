using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduTrack.Web.Controllers;

public class BaseController : Controller
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new Exception("User ID claim not found.");
        }
        return Guid.Parse(userIdClaim.Value);
    }
}
