using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static bool Check(string u, string p) =>
        (u == "admin" && p == "admin") || (u == "user" && p == "user");

    public record LoginDto(string UserName, string Password);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (!Check(dto.UserName, dto.Password))
            return Unauthorized();

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{dto.UserName}:{dto.Password}"));

        Response.Cookies.Append("auth_basic", base64, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                
            SameSite = SameSiteMode.None, 
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        var role = dto.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
        return Ok(new { user = dto.UserName, role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_basic",
            new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        return NoContent();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var isAuth = User?.Identity?.IsAuthenticated ?? false;
        if (!isAuth) return Unauthorized();

        var name = User!.Identity!.Name ?? "";
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        return Ok(new { name, roles });
    }

    [HttpGet("whoami")]
    public IActionResult WhoAmI() => Ok(new
    {
        Authenticated = User?.Identity?.IsAuthenticated ?? false,
        Name = User?.Identity?.Name,
        Roles = User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray()
    });
}