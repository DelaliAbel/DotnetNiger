using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Modération des articles (publication, dépublication).</summary>
public class PostModerationService(AppDbContext db) : IPostModerationService
{
    /// <inheritdoc/>
    public async Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null) return null;
        if (post.AuthorId != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Post.NotAuthorizedPublish);

        post.IsPublished = true;
        post.PublishedAt ??= DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return PostMappers.ToResponse(post);
    }

    /// <inheritdoc/>
    public async Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null) return null;
        if (post.AuthorId != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Post.NotAuthorizedUnpublish);

        post.IsPublished = false;
        post.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return PostMappers.ToResponse(post);
    }
}
