using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Application.Services;

/// <summary>
/// Service for searching across multiple entity types
/// Supports full-text search across Posts, Events, Resources, and Projects
/// </summary>
public class SearchService : ISearchService
{
    private readonly IPostPersistence _postRepository;
    private readonly IEventPersistence _eventRepository;
    private readonly IResourcePersistence _resourceRepository;
    private readonly IProjectPersistence _projectRepository;

    public SearchService(
        IPostPersistence postRepository,
        IEventPersistence eventRepository,
        IResourcePersistence resourceRepository,
        IProjectPersistence projectRepository)
    {
        _postRepository = postRepository;
        _eventRepository = eventRepository;
        _resourceRepository = resourceRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<SearchResultResponse>> SearchPostsAsync(string query, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<SearchResultResponse>();

        var normalizedQuery = query.Trim().ToLowerInvariant();

        // SQL-optimized: Predicate pushed to DB, not GetAllAsync()
        var results = await _postRepository.SearchAsync(
            p => p.IsPublished && (
                p.Title.ToLower().Contains(normalizedQuery) ||
                p.Content.ToLower().Contains(normalizedQuery) ||
                (p.Excerpt != null && p.Excerpt.ToLower().Contains(normalizedQuery))),
            page,
            pageSize);

        return results.Select(p => new SearchResultResponse
        {
            Type = "Post",
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Description = p.Excerpt,
            Slug = p.Slug,
            Excerpt = p.Excerpt,
            CoverImageUrl = p.CoverImageUrl,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<SearchResultResponse>> SearchEventsAsync(string query, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<SearchResultResponse>();

        var normalizedQuery = query.Trim().ToLowerInvariant();

        // SQL-optimized: Predicate pushed to DB
        var results = await _eventRepository.SearchAsync(
            e => e.IsPublished && (
                e.Title.ToLower().Contains(normalizedQuery) ||
                e.Description.ToLower().Contains(normalizedQuery)),
            page,
            pageSize);

        return results.Select(e => new SearchResultResponse
        {
            Type = "Event",
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartDateTime = e.StartDate,
            Slug = e.Slug,
            Excerpt = e.Description.Length > 200 ? e.Description.Substring(0, 200) : e.Description,
            CoverImageUrl = e.CoverImageUrl,
            CreatedAt = e.CreatedAt
        });
    }

    public async Task<IEnumerable<SearchResultResponse>> SearchResourcesAsync(string query, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<SearchResultResponse>();

        var normalizedQuery = query.Trim().ToLowerInvariant();

        // SQL-optimized: Predicate pushed to DB
        var results = await _resourceRepository.SearchAsync(
            r => r.IsApproved && (
                r.Title.ToLower().Contains(normalizedQuery) ||
                r.Description.ToLower().Contains(normalizedQuery)),
            page,
            pageSize);

        return results.Select(r => new SearchResultResponse
        {
            Type = "Resource",
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            Slug = r.Slug,
            Excerpt = r.Description.Length > 200 ? r.Description.Substring(0, 200) : r.Description,
            CoverImageUrl = r.Url,
            CreatedAt = r.CreatedAt
        });
    }

    public async Task<IEnumerable<SearchResultResponse>> SearchProjectsAsync(string query, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<SearchResultResponse>();

        var normalizedQuery = query.Trim().ToLowerInvariant();

        // SQL-optimized: Predicate pushed to DB
        var results = await _projectRepository.SearchAsync(
            p => p.Name.ToLower().Contains(normalizedQuery) ||
                p.Description.ToLower().Contains(normalizedQuery),
            page,
            pageSize);

        return results.Select(p => new SearchResultResponse
        {
            Type = "Project",
            Id = p.Id,
            Title = p.Name,
            Name = p.Name,
            Description = p.Description,
            Slug = p.Slug,
            Excerpt = p.Description.Length > 200 ? p.Description.Substring(0, 200) : p.Description,
            CoverImageUrl = p.GitHubUrl,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<SearchResultResponse>> SearchAsync(SearchQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Enumerable.Empty<SearchResultResponse>();

        var results = new List<SearchResultResponse>();

        // Search by specific type if provided
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            return request.Type.ToLowerInvariant() switch
            {
                "post" => await SearchPostsAsync(request.Query, request.Page, request.PageSize),
                "event" => await SearchEventsAsync(request.Query, request.Page, request.PageSize),
                "resource" => await SearchResourcesAsync(request.Query, request.Page, request.PageSize),
                "project" => await SearchProjectsAsync(request.Query, request.Page, request.PageSize),
                _ => Enumerable.Empty<SearchResultResponse>()
            };
        }

        // Search all types and aggregate results
        var posts = await SearchPostsAsync(request.Query, 1, request.PageSize);
        var events = await SearchEventsAsync(request.Query, 1, request.PageSize);
        var resources = await SearchResourcesAsync(request.Query, 1, request.PageSize);
        var projects = await SearchProjectsAsync(request.Query, 1, request.PageSize);

        results.AddRange(posts);
        results.AddRange(events);
        results.AddRange(resources);
        results.AddRange(projects);

        // Sort by recency and paginate
        return results
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
    }
}
