using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.Notifications;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Commandes de modification des articles.</summary>
public class PostCommandService(AppDbContext db, INotificationService notification, ILogger<PostCommandService> logger, ICertificateService certificateService) : IPostCommandService
{
    /// <inheritdoc/>
    public async Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName, bool isAdmin, bool isCollaborator)
    {
        var (canCreate, forceUnpublished, error) = await certificateService.CanCreateContentAsync(authorId, isAdmin, isCollaborator);
        if (!canCreate)
        {
            if (error != null) throw new InvalidOperationException(error);
            throw new UnauthorizedAccessException();
        }
        if (forceUnpublished) request.IsPublished = false;

        var slug = await EnsureUniqueSlugAsync(SlugGenerator.GenerateSlug(request.Title));
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Excerpt = request.Excerpt,
            CoverImageUrl = request.CoverImageUrl,
            AuthorId = authorId,
            AuthorName = authorName,
            PostType = request.PostType,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await AssignCategories(post, request.CategoryIds);
        await AssignTags(post, request.TagNames);
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        if (request.IsPublished)
        {
            try { await notification.NotifyNewPostAsync(post.Title, post.AuthorName); }
            catch (Exception ex) { logger.LogWarning(ex, "Échec notification pour le nouvel article {Title}", post.Title); }
        }

        return PostMappers.ToResponse(post);
    }

    /// <inheritdoc/>
    public async Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin)
    {
        var post = await db.Posts.Include(p => p.PostCategories).Include(p => p.PostTags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return null;
        if (post.AuthorId != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Post.NotAuthorizedModify);

        post.Title = request.Title;
        var newSlug = SlugGenerator.GenerateSlug(request.Title);
        if (newSlug != post.Slug)
            newSlug = await EnsureUniqueSlugAsync(newSlug, id);
        post.Slug = newSlug;
        post.Content = request.Content;
        post.Excerpt = request.Excerpt;
        post.CoverImageUrl = request.CoverImageUrl;
        post.PostType = request.PostType;
        post.IsPublished = request.IsPublished;
        if (request.IsPublished && post.PublishedAt is null)
            post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        db.PostCategories.RemoveRange(post.PostCategories);
        db.PostTags.RemoveRange(post.PostTags);
        await AssignCategories(post, request.CategoryIds);
        await AssignTags(post, request.TagNames);
        await db.SaveChangesAsync();
        return PostMappers.ToResponse(post);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null) return false;
        if (post.AuthorId != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Post.NotAuthorizedDelete);
        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<PostResponse?> IncrementViewCountAsync(Guid id)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null) return null;
        post.ViewCount++;
        await db.SaveChangesAsync();
        return PostMappers.ToResponse(post);
    }

    private async Task AssignCategories(Post post, List<Guid> categoryIds)
    {
        var categories = await db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync();
        foreach (var cat in categories)
            post.PostCategories.Add(new PostCategory { PostId = post.Id, CategoryId = cat.Id });
    }

    private async Task AssignTags(Post post, List<string> tagNames)
    {
        var names = tagNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0) return;

        var slugs = names.Select(SlugGenerator.GenerateSlug).ToHashSet();
        var existingTags = await db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync();
        var existingBySlug = existingTags.ToDictionary(t => t.Slug);

        foreach (var name in names)
        {
            var slug = SlugGenerator.GenerateSlug(name);
            if (!existingBySlug.TryGetValue(slug, out var tag))
            {
                tag = new Tag { Id = Guid.NewGuid(), Name = name, Slug = slug };
                db.Tags.Add(tag);
                existingBySlug[slug] = tag;
            }
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, Guid? excludeId = null)
    {
        var existing = await db.Posts.AsNoTracking()
            .Where(p => p.Slug.StartsWith(baseSlug) && (excludeId == null || p.Id != excludeId.Value))
            .Select(p => p.Slug)
            .ToListAsync();

        if (!existing.Contains(baseSlug)) return baseSlug;

        for (var i = 1; i < 100; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!existing.Contains(candidate)) return candidate;
        }

        return $"{baseSlug}-{Guid.NewGuid():N}";
    }
}
