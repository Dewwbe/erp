using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MiniErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.Identity?.Name;
        var role = User.FindFirstValue(ClaimTypes.Role);
        var fullName = User.FindFirst("fullName")?.Value;

        return Ok(new { email, fullName, role });
    }
}
