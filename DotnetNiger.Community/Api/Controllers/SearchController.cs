using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour la recherche dans le service Community
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SearchController : ApiControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Rechercher du contenu avec pagination
    /// Utilise une requête optimisée côté database (LIKE + pagination)
    /// </summary>
    /// <param name="query">Terme de recherche (min 2 caractères)</param>
    /// <param name="page">Numéro de page (défaut 1)</param>
    /// <param name="pageSize">Resultats par page (défaut 20, max 100)</param>
    /// <returns>Résultats paginés de la recherche</returns>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequestProblem("Terme de recherche requis");

        if (query.Length < 2)
            return BadRequestProblem("La recherche doit contenir au moins 2 caractères");

        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 20;

        if (pageSize > 100)
            pageSize = 100;

        // Utiliser SearchService pour requêtes côté base de données optimisées
        var postsResults = await _searchService.SearchPostsAsync(query, page, pageSize);
        var eventsResults = await _searchService.SearchEventsAsync(query, page, pageSize);
        var resourcesResults = await _searchService.SearchResourcesAsync(query, page, pageSize);
        var projectsResults = await _searchService.SearchProjectsAsync(query, page, pageSize);

        var searchResults = new SearchResultsDto
        {
            Query = query,
            Posts = postsResults.Select(p => new SearchPostResultDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt
            }).ToList(),
            Events = eventsResults.Select(e => new SearchEventResultDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDateTime = e.StartDateTime
            }).ToList(),
            Projects = projectsResults.Select(pr => new SearchProjectResultDto
            {
                Id = pr.Id,
                Name = pr.Name,
                Description = pr.Description,
                CreatedAt = pr.CreatedAt
            }).ToList(),
            Resources = resourcesResults.Select(r => new SearchResourceResultDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            }).ToList(),
            TotalResults = postsResults.Count() + eventsResults.Count() + projectsResults.Count() + resourcesResults.Count(),
            ExecutedAtUtc = DateTime.UtcNow
        };

        return Success(searchResults);
    }
}

