using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class PostModerationService : IPostModerationService
{
    private readonly DotnetNigerDbContext _db;

    public PostModerationService(DotnetNigerDbContext db) => _db = db;

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
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt);
}
