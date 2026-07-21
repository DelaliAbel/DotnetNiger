using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

public class SearchService : ISearchService
{
    private readonly DotnetNigerDbContext _db;

    public SearchService(DotnetNigerDbContext db) => _db = db;

    public async Task<SearchResultResponse> SearchAsync(SearchQueryRequest request)
    {
        var query = (request.Query ?? "").ToLower().Trim();
        if (string.IsNullOrWhiteSpace(query))
            return new SearchResultResponse { Type = "empty" };

        var results = new List<SearchResultResponse>();

        if (request.Type == null || request.Type == "posts")
        {
            var posts = await _db.Posts.AsNoTracking()
                .Where(p => p.Status == PostStatus.Published
                    && (p.Title.ToLower().Contains(query) || p.Content.ToLower().Contains(query)))
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
                    && (e.Title.ToLower().Contains(query) || e.Description.ToLower().Contains(query)))
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
                    && (r.Title.ToLower().Contains(query) || r.Description.ToLower().Contains(query)))
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

        if (results.Count == 0)
            return new SearchResultResponse { Type = "no_results", Title = "Aucun résultat trouvé" };

        return results[0];
    }
}
