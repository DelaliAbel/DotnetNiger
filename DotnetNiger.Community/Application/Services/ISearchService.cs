using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Recherche unifiée dans les articles, événements et ressources.</summary>
public interface ISearchService
{
    /// <summary>Effectue une recherche multi-contenus avec pagination.</summary>
    Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request);
}
