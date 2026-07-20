using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/test")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "DotnetNiger.Server",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("checkpassword")]
    public async Task<IActionResult> CheckPassword(
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] SignInManager<ApplicationUser> signInManager)
    {
        var results = new List<object>();
        foreach (var (email, password) in new[] {
            ("admin@dotnetniger.com", "Admin@123456"),
            ("admin@dotnetniger.com", "admin"),
            ("admin@dotnetniger.com", "Admin123456"),
            ("test@dotnetniger.com", "Test@123456"),
            ("test@dotnetniger.com", "test"),
            ("test@dotnetniger.com", "Test123456"),
        })
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                results.Add(new { Email = email, Password = password, Found = false });
                continue;
            }
            var check = await userManager.CheckPasswordAsync(user, password);
            results.Add(new
            {
                Email = email,
                Password = password,
                Found = true,
                CheckPasswordResult = check,
            });
        }
        return Ok(results);
    }
}
