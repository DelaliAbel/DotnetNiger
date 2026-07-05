using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Requêtes de consultation des articles.</summary>
public class PostQueryService(AppDbContext db) : IPostQueryService
{
    /// <inheritdoc/>
    public async Task<PaginatedResponse<PostResponse>> GetAllAsync(string? published, string? category, string? tag, string? query, int page, int pageSize, Guid? after)
    {
        var q = BuildQuery();

        if (published == "true") q = q.Where(p => p.IsPublished);
        else if (published == "false") q = q.Where(p => !p.IsPublished);
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.PostCategories.Any(pc => pc.Category.Slug == category));
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(p => p.PostTags.Any(pt => pt.Tag.Slug == tag));
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => p.Title.Contains(query) || p.Content.Contains(query));

        int total = await q.CountAsync();
        List<Post> items;

        if (after.HasValue)
            items = await q.Where(p => p.Id > after.Value).OrderBy(p => p.Id).Take(pageSize).ToListAsync();
        else
            items = await q.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedResponse<PostResponse> { Items = items.Select(PostMappers.ToResponse).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc/>
    public async Task<PostResponse?> GetByIdAsync(Guid id)
    {
        var post = await BuildQuery().FirstOrDefaultAsync(p => p.Id == id);
        return post is null ? null : PostMappers.ToResponse(post);
    }

    /// <inheritdoc/>
    public async Task<PostResponse?> GetBySlugAsync(string slug)
    {
        var post = await BuildQuery().FirstOrDefaultAsync(p => p.Slug == slug);
        return post is null ? null : PostMappers.ToResponse(post);
    }

    private IQueryable<Post> BuildQuery() => db.Posts.AsNoTracking()
        .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
        .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
        .AsSplitQuery();
}
