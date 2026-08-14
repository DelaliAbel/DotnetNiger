using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Posts;

/// <summary>Service de consultation des articles (requêtes en lecture seule).</summary>
public class PostQueryService : IPostQueryService
{
    private readonly DotnetNigerDbContext _db;

    public PostQueryService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste paginée des articles avec filtres optionnels.</summary>
    public async Task<PaginatedResponse<PostResponse>> GetAllAsync(
        string? published, string? category, string? tag,
        string? query, int page, int pageSize, Guid? after = null, Guid? authorId = null, CancellationToken ct = default)
    {
        var q = _db.Posts
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .AsNoTracking();

        if (published == "true") q = q.Where(p => p.Status == PostStatus.Published);
        else if (published == "false") q = q.Where(p => p.Status == PostStatus.Draft || p.Status == PostStatus.PendingReview);
        else if (published != null) q = q.Where(p => p.Status == PostStatus.Published);

        if (authorId.HasValue) q = q.Where(p => p.AuthorId == authorId.Value);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || (p.Content != null && p.Content.Contains(query)));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.PostCategories.Any(pc => pc.Category.Slug == category || pc.Category.Name == category));
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(p => p.PostTags.Any(pt => pt.Tag.Slug == tag || pt.Tag.Name == tag));

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResponse<PostResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Récupère un article par son identifiant.</summary>
    public async Task<PostResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _db.Posts.AsNoTracking()
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        return post == null ? null : MapToResponse(post);
    }

    /// <summary>Récupère un article par son slug.</summary>
    public async Task<PostResponse?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await _db.Posts.AsNoTracking()
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);
        return post == null ? null : MapToResponse(post);
    }

    private static PostResponse MapToResponse(Post p)
    {
        var categories = p.PostCategories?.Select(pc => new CategoryResponse(
            pc.Category.Id, pc.Category.Name, pc.Category.Slug,
            pc.Category.Description, pc.Category.IconUrl, pc.Category.PostCount)).ToList() ?? [];

        var tags = p.PostTags?.Select(pt => new TagResponse
        {
            Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug, UsageCount = pt.Tag.UsageCount
        }).ToList() ?? [];

        return new PostResponse(
            p.Id, p.Title, p.Slug, p.Content, p.Excerpt, p.CoverImageUrl,
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt,
            p.AuthorName, p.AuthorAvatar, p.PostType, p.ViewCount,
            categories, tags);
    }
}
