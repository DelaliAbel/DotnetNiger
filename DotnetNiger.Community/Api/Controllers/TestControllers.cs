using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "ok",
            service = "DotnetNiger.Community",
            utcTime = DateTime.UtcNow
        });
    }
}
