using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour la recherche dans le service Community
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly IProjectService _projectService;
    private readonly IResourceService _resourceService;

    public SearchController(
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
    /// Rechercher du contenu
    /// </summary>
    /// <param name="query">Terme de recherche</param>
    /// <returns>Résultats de la recherche</returns>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Terme de recherche requis" });

        try
        {
            // Recherche dans les posts
            var posts = await _postService.GetAllPublishedPostsAsync(1, 10);
            var postsResults = posts
                .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                           p.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Recherche dans les événements
            var events = await _eventService.GetAllEventsAsync(1, 10);
            var eventsResults = events
                .Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                           e.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Recherche dans les projets
            var projects = await _projectService.GetAllProjectsAsync(1, 10);
            var projectsResults = projects
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                           p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Recherche dans les ressources
            var resources = await _resourceService.GetAllResourcesAsync(1, 10);
            var resourcesResults = resources
                .Where(r => r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                           r.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(new
            {
                query = query,
                results = new
                {
                    posts = postsResults,
                    events = eventsResults,
                    projects = projectsResults,
                    resources = resourcesResults
                },
                totalResults = postsResults.Count + eventsResults.Count + projectsResults.Count + resourcesResults.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la recherche", error = ex.Message });
        }
    }
}
