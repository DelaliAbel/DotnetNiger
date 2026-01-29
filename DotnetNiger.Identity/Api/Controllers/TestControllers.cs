using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("test")]
    [Authorize]
    public IActionResult GetTest()
    {
        return Ok(new[] { new { Id = 1, Title = "Test Ok" } });
    }

    [HttpGet("health")]
    [Authorize]
    public IActionResult GetHealth()
    {
        return Ok(new[] { new { Id = 2, Title = "Health Ok" } });
    }
}
