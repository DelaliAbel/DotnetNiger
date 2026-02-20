using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{

    [HttpGet("couranDaga")]
    public IActionResult GetAdmin()
    {
        return Ok(new[] {
             new { Id = 1, Title = "Mahamadou GARBA ZAKOU" }, 
             new { Id = 2, Title = "Salif" }, 
             new { Id = 3, Title = "Moudi" },  
             new { Id = 6, Title = "Hamidou" },  
             new { Id = 7, Title = "Kadri" },  
             new { Id = 8, Title = "Mohamed" },  
             });
    }
}
