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
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return null;
        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    /// <summary>Retire un article de publication (passe le statut à Draft).</summary>
    public async Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return null;
        post.Status = PostStatus.Draft;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    private static PostResponse MapToResponse(Post p) =>
        new(p.Id, p.Title, p.Slug, p.Content, p.Excerpt, p.CoverImageUrl,
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt,
            p.AuthorName, p.AuthorAvatar, p.PostType, p.ViewCount, [], []);
}
