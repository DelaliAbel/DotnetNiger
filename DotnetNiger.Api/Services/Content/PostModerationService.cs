using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de modération des articles (publication/retrait).</summary>
public class PostModerationService : IPostModerationService
{
    private readonly DotnetNigerDbContext _db;

    public PostModerationService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Publie un article (passe le statut à Published).</summary>
    public async Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await _db.Posts
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null) return null;
        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à publier cet article.");
        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    /// <summary>Retire un article de publication (passe le statut à Draft).</summary>
    public async Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await _db.Posts
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null) return null;
        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à dépublier cet article.");
        post.Status = PostStatus.Draft;
        post.PublishedAt = null;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    private static PostResponse MapToResponse(Post p) =>
        new(p.Id, p.Title, p.Slug, p.Content, p.Excerpt, p.CoverImageUrl,
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt,
            p.AuthorName, p.AuthorAvatar, p.PostType, p.ViewCount,
            p.PostCategories?.Select(pc => new CategoryResponse(pc.Category.Id, pc.Category.Name, pc.Category.Slug, pc.Category.Description ?? "")).ToList() ?? [],
            p.PostTags?.Select(pt => new TagResponse { Id = pt.Tag.Id, Name = pt.Tag.Name, Slug = pt.Tag.Slug }).ToList() ?? []);
}
