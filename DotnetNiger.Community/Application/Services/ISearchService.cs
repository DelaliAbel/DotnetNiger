using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Recherche unifiée dans les articles, événements et ressources.</summary>
public interface ISearchService
{
    /// <summary>Effectue une recherche multi-contenus avec pagination.</summary>
    Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request);
}
