using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class SearchService(AppDbContext db) : ISearchService
{
    public async Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request)
    {
        var query = request.Query?.Trim();
        var type = request.Type?.Trim();
        IQueryable<SearchResultResponse>? combined = null;

        if (string.IsNullOrWhiteSpace(type) || type == "Post")
        {
            var posts = db.Posts.Where(p => p.IsPublished)
                .Select(p => new SearchResultResponse
                {
                    Type = "Post", Id = p.Id, Title = p.Title, Slug = p.Slug,
                    Excerpt = p.Excerpt, Content = p.Content, CoverImageUrl = p.CoverImageUrl,
                    CreatedAt = p.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                posts = posts.Where(p => p.Title!.Contains(query) || p.Content!.Contains(query));
            combined = posts;
        }

        if (string.IsNullOrWhiteSpace(type) || type == "Event")
        {
            var events = db.Events.Where(e => e.IsPublished)
                .Select(e => new SearchResultResponse
                {
                    Type = "Event", Id = e.Id, Title = e.Title, Slug = e.Slug,
                    Description = e.Description, CoverImageUrl = e.CoverImageUrl,
                    StartDateTime = e.StartDate, CreatedAt = e.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                events = events.Where(e => e.Title!.Contains(query) || e.Description!.Contains(query));
            combined = combined == null ? events : combined.Concat(events);
        }

        if (string.IsNullOrWhiteSpace(type) || type == "Resource")
        {
            var resources = db.Resources
                .Select(r => new SearchResultResponse
                {
                    Type = "Resource", Id = r.Id, Title = r.Title, Slug = r.Slug,
                    Description = r.Description, CreatedAt = r.CreatedAt
                });
            if (!string.IsNullOrWhiteSpace(query))
                resources = resources.Where(r => r.Title!.Contains(query) || r.Description!.Contains(query));
            combined = combined == null ? resources : combined.Concat(resources);
        }

        if (combined is null)
            return new PaginatedResponse<SearchResultResponse>
            {
                Items = [], TotalCount = 0, Page = request.Page, PageSize = request.PageSize
            };

        var total = await combined.CountAsync();
        var items = await combined
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<SearchResultResponse>
        {
            Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize
        };
    }
}
