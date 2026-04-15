using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour récupérer les statistiques du service Community
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/stats")]
public class StatsController : ApiControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IEventService _eventService;

    public StatsController(
        IStatisticsService statisticsService,
        IEventService eventService)
    {
        _statisticsService = statisticsService;
        _eventService = eventService;
    }

    /// <summary>
    /// Récupérer les statistiques générales
    /// </summary>
    /// <returns>Statistiques du service</returns>
    [HttpGet]
    [OutputCache(PolicyName = "StatsPolicy")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _statisticsService.GetCommunityStatsAsync();
        var upcomingEvents = await _eventService.GetUpcomingEventsAsync(1000);

        return Success(new
        {
            totalPosts = stats.TotalPosts,
            totalEvents = stats.TotalEvents,
            totalProjects = stats.TotalProjects,
            totalResources = stats.TotalResources,
            upcomingEvents = upcomingEvents.Count(),
            activeProjects = stats.TotalProjects,
            lastUpdated = DateTime.UtcNow
        });
    }
}
