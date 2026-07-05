using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Recherche unifiée dans les articles publiés, événements publiés et ressources.</summary>
public class SearchService(AppDbContext db) : ISearchService
{
    /// <summary>Cherche dans tous les types de contenu, avec pagination et filtre par type optionnel.</summary>
    public async Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request)
    {
        request.Page = Math.Max(1, request.Page);
        request.PageSize = Math.Clamp(request.PageSize, 1, ValidationConstants.MaxPageSize);

        var query = request.Query?.Trim();
        var type = request.Type?.Trim();

        var postsQuery = string.IsNullOrWhiteSpace(type) || type == "Post"
            ? BuildPostQuery(query)
            : null;

        var eventsQuery = string.IsNullOrWhiteSpace(type) || type == "Event"
            ? BuildEventQuery(query)
            : null;

        var resourcesQuery = string.IsNullOrWhiteSpace(type) || type == "Resource"
            ? BuildResourceQuery(query)
            : null;

        var allResults = Enumerable.Empty<SearchResultResponse>()
            .AsQueryable();

        if (postsQuery != null)
            allResults = allResults.Concat(postsQuery);
        if (eventsQuery != null)
            allResults = allResults.Concat(eventsQuery);
        if (resourcesQuery != null)
            allResults = allResults.Concat(resourcesQuery);

        var total = await allResults.CountAsync();

        var items = await allResults
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<SearchResultResponse>
        {
            Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }

    private IQueryable<SearchResultResponse> BuildPostQuery(string? query)
    {
        var q = db.Posts.AsNoTracking().Where(p => p.IsPublished)
            .Select(p => new SearchResultResponse
            {
                Type = "Post", Id = p.Id, Title = p.Title, Slug = p.Slug,
                Excerpt = p.Excerpt, Content = p.Content, CoverImageUrl = p.CoverImageUrl,
                CreatedAt = p.CreatedAt
            });
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p =>
                (p.Title != null && p.Title.Contains(query)) ||
                (p.Content != null && p.Content.Contains(query)));
        return q;
    }

    private IQueryable<SearchResultResponse> BuildEventQuery(string? query)
    {
        var q = db.Events.AsNoTracking().Where(e => e.IsPublished)
            .Select(e => new SearchResultResponse
            {
                Type = "Event", Id = e.Id, Title = e.Title, Slug = e.Slug,
                Description = e.Description, CoverImageUrl = e.CoverImageUrl,
                StartDateTime = e.StartDate, CreatedAt = e.CreatedAt
            });
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e =>
                (e.Title != null && e.Title.Contains(query)) ||
                (e.Description != null && e.Description.Contains(query)));
        return q;
    }

    private IQueryable<SearchResultResponse> BuildResourceQuery(string? query)
    {
        var q = db.Resources.AsNoTracking()
            .Select(r => new SearchResultResponse
            {
                Type = "Resource", Id = r.Id, Title = r.Title, Slug = r.Slug,
                Description = r.Description, CreatedAt = r.CreatedAt
            });
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r =>
                (r.Title != null && r.Title.Contains(query)) ||
                (r.Description != null && r.Description.Contains(query)));
        return q;
    }
}
