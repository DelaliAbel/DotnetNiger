using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour récupérer les statistiques du service Community
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StatsController : ApiControllerBase
{
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly IProjectService _projectService;
    private readonly IResourceService _resourceService;

    public StatsController(
        IPostService postService,
        IEventService eventService,
        IProjectService projectService,
        IResourceService resourceService)
    {
        _postService = postService;
        _eventService = eventService;
        _projectService = projectService;
        _resourceService = resourceService;
    }

    /// <summary>
    /// Récupérer les statistiques générales
    /// </summary>
    /// <returns>Statistiques du service</returns>
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        var posts = await _postService.GetAllPublishedPostsAsync(1, 1000);
        var events = await _eventService.GetAllEventsAsync(1, 1000);
        var projects = await _projectService.GetAllProjectsAsync(1, 1000);
        var resources = await _resourceService.GetAllResourcesAsync(1, 1000);

        return Success(new
        {
            totalPosts = posts.Count(),
            totalEvents = events.Count(),
            totalProjects = projects.Count(),
            totalResources = resources.Count(),
            upcomingEvents = events.Count(e => e.StartDate > DateTime.UtcNow),
            activeProjects = projects.Count(),
            lastUpdated = DateTime.UtcNow
        });
    }
}
