using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        return Ok(new[] { new { Id = 1, Title = "Test Ok" } });
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new[] { new { Id = 2, Title = "Health Ok" } });
    }
}
