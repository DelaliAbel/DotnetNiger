using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour récupérer les statistiques du service Community
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    /// <summary>
    /// Récupérer les statistiques générales
    /// </summary>
    /// <returns>Statistiques du service</returns>
    [HttpGet]
    public IActionResult GetStatistics()
    {
        return Ok(new
        {
            totalPosts = 0,
            totalEvents = 0,
            totalProjects = 0,
            totalResources = 0,
            totalUsers = 0,
            totalComments = 0,
            lastUpdated = DateTime.UtcNow
        });
    }
}
