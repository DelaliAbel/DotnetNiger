using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class SearchService(AppDbContext db) : ISearchService
{
    public async Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request)
    {
        request.Page = Math.Max(1, request.Page);
        request.PageSize = Math.Clamp(request.PageSize, 1, ValidationConstants.MaxPageSize);

        var query = request.Query?.Trim();
        var type = request.Type?.Trim();

        var maxPerType = request.Page * request.PageSize * 3;

        var results = new List<SearchResultResponse>();

        if (string.IsNullOrWhiteSpace(type) || type == "Post")
        {
            var posts = db.Posts.AsNoTracking().Where(p => p.IsPublished)
                .Select(p => new SearchResultResponse
                {
                    Type = "Post", Id = p.Id, Title = p.Title, Slug = p.Slug,
                    Excerpt = p.Excerpt, Content = p.Content, CoverImageUrl = p.CoverImageUrl,
                    CreatedAt = p.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                posts = posts.Where(p => p.Title != null && p.Title.Contains(query) || p.Content != null && p.Content.Contains(query));
            results.AddRange(await posts.OrderByDescending(p => p.CreatedAt).Take(maxPerType).ToListAsync());
        }

        if (string.IsNullOrWhiteSpace(type) || type == "Event")
        {
            var events = db.Events.AsNoTracking().Where(e => e.IsPublished)
                .Select(e => new SearchResultResponse
                {
                    Type = "Event", Id = e.Id, Title = e.Title, Slug = e.Slug,
                    Description = e.Description, CoverImageUrl = e.CoverImageUrl,
                    StartDateTime = e.StartDate, CreatedAt = e.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                events = events.Where(e => e.Title != null && e.Title.Contains(query) || e.Description != null && e.Description.Contains(query));
            results.AddRange(await events.OrderByDescending(e => e.CreatedAt).Take(maxPerType).ToListAsync());
        }

        if (string.IsNullOrWhiteSpace(type) || type == "Resource")
        {
            var resources = db.Resources.AsNoTracking()
                .Select(r => new SearchResultResponse
                {
                    Type = "Resource", Id = r.Id, Title = r.Title, Slug = r.Slug,
                    Description = r.Description, CreatedAt = r.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                resources = resources.Where(r => r.Title != null && r.Title.Contains(query) || r.Description != null && r.Description.Contains(query));
            results.AddRange(await resources.OrderByDescending(r => r.CreatedAt).Take(maxPerType).ToListAsync());
        }

        var total = results.Count;
        var items = results
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResponse<SearchResultResponse>
        {
            Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }
}
