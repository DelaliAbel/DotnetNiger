using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Search;

/// <summary>Service de recherche global dans les articles, événements et ressources.</summary>
public class SearchService : ISearchService
{
    private readonly DotnetNigerDbContext _db;

    public SearchService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Recherche parmi les contenus publiés selon la requête et le type.</summary>
    public async Task<PaginatedResponse<SearchResultResponse>> SearchAsync(SearchQueryRequest request)
    {
        var query = (request.Query ?? "").ToLower().Trim();
        if (string.IsNullOrWhiteSpace(query))
            return new PaginatedResponse<SearchResultResponse>([], 0, 1, 15);

        var results = new List<SearchResultResponse>();

        if (request.Type == null || request.Type == "posts")
        {
            var posts = await _db.Posts.AsNoTracking()
                .Where(p => p.Status == PostStatus.Published
                    && (p.Title.ToLower().Contains(query) || (p.Content != null && p.Content.ToLower().Contains(query))))
                .Take(5)
                .ToListAsync();
            foreach (var p in posts)
                results.Add(new SearchResultResponse
                {
                    Type = "post", Id = p.Id, Title = p.Title,
                    Slug = p.Slug, Excerpt = p.Excerpt,
                    CoverImageUrl = p.CoverImageUrl, CreatedAt = p.CreatedAt
                });
        }

        if (request.Type == null || request.Type == "events")
        {
            var events = await _db.Events.AsNoTracking()
                .Where(e => e.Status == EventStatus.Published
                    && (e.Title.ToLower().Contains(query) || (e.Description != null && e.Description.ToLower().Contains(query))))
                .Take(5)
                .ToListAsync();
            foreach (var e in events)
                results.Add(new SearchResultResponse
                {
                    Type = "event", Id = e.Id, Title = e.Title,
                    Slug = e.Slug, Description = e.Description,
                    StartDateTime = e.StartDate, CreatedAt = e.CreatedAt
                });
        }

        if (request.Type == null || request.Type == "resources")
        {
            var resources = await _db.Resources.AsNoTracking()
                .Where(r => r.Status == ResourceStatus.Published
                    && (r.Title.ToLower().Contains(query) || (r.Description != null && r.Description.ToLower().Contains(query))))
                .Take(5)
                .ToListAsync();
            foreach (var r in resources)
                results.Add(new SearchResultResponse
                {
                    Type = "resource", Id = r.Id, Title = r.Title,
                    Slug = r.Slug, Description = r.Description,
                    CreatedAt = r.CreatedAt
                });
        }

        var total = results.Count;
        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 15;
        var paged = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResponse<SearchResultResponse>(paged, total, page, pageSize);
    }
}
