using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/diagnostics")]
public class DiagnosticsController : ApiControllerBase
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Success(new { status = "ok", service = "DotnetNiger.Identity", utcTime = DateTime.UtcNow });
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Success(new { status = "healthy", service = "DotnetNiger.Identity", utcTime = DateTime.UtcNow });
    }
}
