using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services.Interfaces;

/// <summary>
/// Service for searching across multiple entity types (Posts, Events, Resources, Projects)
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches posts by query string
    /// </summary>
    Task<IEnumerable<SearchResultResponse>> SearchPostsAsync(string query, int page = 1, int pageSize = 10);

    /// <summary>
    /// Searches events by query string
    /// </summary>
    Task<IEnumerable<SearchResultResponse>> SearchEventsAsync(string query, int page = 1, int pageSize = 10);

    /// <summary>
    /// Searches resources by query string
    /// </summary>
    Task<IEnumerable<SearchResultResponse>> SearchResourcesAsync(string query, int page = 1, int pageSize = 10);

    /// <summary>
    /// Searches projects by query string
    /// </summary>
    Task<IEnumerable<SearchResultResponse>> SearchProjectsAsync(string query, int page = 1, int pageSize = 10);

    /// <summary>
    /// Performs a unified search across all entity types
    /// </summary>
    Task<IEnumerable<SearchResultResponse>> SearchAsync(SearchQueryRequest request);
}
