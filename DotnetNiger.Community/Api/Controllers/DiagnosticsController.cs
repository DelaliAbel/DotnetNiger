using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Points de contrôle pour vérifier l'état de santé du service.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test")]
public class DiagnosticsController : ControllerBase
{
    /// <summary>Retourne l'état de santé actuel du service Community.</summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "DotnetNiger.Community",
            timestamp = DateTime.UtcNow
        });
    }
}
