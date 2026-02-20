using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    // [HttpGet("admin")]
    // public IActionResult GetAdmin()
    // {
    //     return Ok(new[] {
    //          new { Id = 3, Title = "Mahamadou GARBA ZAKOU" }, 
    //          new { Id = 4, Title = "Kofi" }, 
    //          new { Id = 5, Title = "Moudi" }  
    //          });
    // }
}
