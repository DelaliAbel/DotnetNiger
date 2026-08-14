using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de recherche global.</summary>
public interface ISearchService
{
    /// <summary>Effectue une recherche parmi les contenus.</summary>
    Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request, CancellationToken ct = default);
}
