using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.General;

/// <summary>Interface du service de recherche global.</summary>
public interface ISearchService
{
    /// <summary>Effectue une recherche parmi les contenus.</summary>
    Task<SearchResultResponse> SearchAsync(SearchQueryRequest request);
}
