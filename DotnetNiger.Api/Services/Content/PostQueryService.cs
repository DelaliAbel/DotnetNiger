using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

public class PostQueryService : IPostQueryService
{
    private readonly DotnetNigerDbContext _db;

    public PostQueryService(DotnetNigerDbContext db) => _db = db;

    public async Task<PaginatedResponse<PostResponse>> GetAllAsync(
        string? published, string? category, string? tag,
        string? query, int page, int pageSize, Guid? after = null, Guid? authorId = null)
    {
        var q = _db.Posts.AsNoTracking();

        if (published == "true") q = q.Where(p => p.Status == PostStatus.Published);
        else if (published == "false") q = q.Where(p => p.Status == PostStatus.Draft || p.Status == PostStatus.PendingReview);

        if (authorId.HasValue) q = q.Where(p => p.AuthorId == authorId.Value);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || (p.Content != null && p.Content.Contains(query)));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<PostResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    public async Task<PostResponse?> GetByIdAsync(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        return post == null ? null : MapToResponse(post);
    }

    public async Task<PostResponse?> GetBySlugAsync(string slug)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        return post == null ? null : MapToResponse(post);
    }

    private static PostResponse MapToResponse(Post p) =>
        new(p.Id, p.Title, p.Slug, p.Content, p.Excerpt, p.CoverImageUrl,
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt);
}
