using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Posts;

/// <summary>Service de création, modification et suppression des articles.</summary>
public class PostCommandService : IPostCommandService
{
    private readonly DotnetNigerDbContext _db;

    public PostCommandService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée un nouvel article avec ses tags et catégories.</summary>
    public async Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName, bool isAdmin, bool isCollaborator)
    {
        var slug = await GenerateUniqueSlug(request.Slug, request.Title);

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
            AuthorAvatar = "",
            PostType = request.PostType,
            IsPublished = request.IsPublished,
            Status = isAdmin || isCollaborator ? PostStatus.Published : PostStatus.Draft
        };

        await SyncPostTagsAsync(post, request.TagNames, request.TagIds);
        await SyncPostCategoriesAsync(post, request.CategoryIds);

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    /// <summary>Met à jour un article existant.</summary>
    public async Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin)
    {
        var post = await _db.Posts
            .Include(p => p.PostTags)
            .Include(p => p.PostCategories)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return null;

        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cet article.");

        if (request.Title != null) post.Title = request.Title;
        if (request.Slug != null) post.Slug = await EnsureUniqueSlug(request.Slug, post.Id);
        if (request.Content != null) post.Content = request.Content;
        if (request.Excerpt != null) post.Excerpt = request.Excerpt;
        if (request.CoverImageUrl != null) post.CoverImageUrl = request.CoverImageUrl;
        if (request.PostType != null) post.PostType = request.PostType;
        if (request.IsPublished.HasValue) post.IsPublished = request.IsPublished.Value;

        if (request.TagNames != null)
            await SyncPostTagsAsync(post, request.TagNames, null);
        if (request.CategoryIds != null)
            await SyncPostCategoriesAsync(post, request.CategoryIds);

        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(post);
    }

    /// <summary>Supprime un article de façon permanente.</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return false;
        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cet article.");

        var postId = post.Id;
        var tags = await _db.PostTags.Where(t => t.PostId == postId).ToListAsync();
        _db.PostTags.RemoveRange(tags);
        var categories = await _db.PostCategories.Where(c => c.PostId == postId).ToListAsync();
        _db.PostCategories.RemoveRange(categories);
        var comments = await _db.Comments.Where(c => c.PostId == postId).ToListAsync();
        _db.Comments.RemoveRange(comments);

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Incrémente le compteur de vues d'un article.</summary>
    public async Task<PostResponse?> IncrementViewCountAsync(Guid id)
    {
        var affected = await _db.Posts
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.ViewCount, p => p.ViewCount + 1)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));

        if (affected == 0) return null;

        var post = await _db.Posts.FindAsync(id);
        return post == null ? null : MapToResponse(post);
    }

    /// <summary>Soumet un article pour modération.</summary>
    public async Task SubmitForReviewAsync(Guid id)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Article non trouvé");
        post.Status = PostStatus.PendingReview;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Publie un article.</summary>
    public async Task PublishAsync(Guid id)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Article non trouvé");
        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Archive un article.</summary>
    public async Task ArchiveAsync(Guid id)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Article non trouvé");
        post.Status = PostStatus.Archived;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task SyncPostTagsAsync(Post post, List<string>? tagNames, List<Guid>? tagIds)
    {
        if (post.PostTags.Count != 0)
        {
            _db.Set<PostTag>().RemoveRange(post.PostTags);
            post.PostTags.Clear();
        }

        var tagsToLink = new List<Tag>();

        if (tagIds?.Count > 0)
        {
            var existing = await _db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            tagsToLink.AddRange(existing);
        }

        if (tagNames?.Count > 0)
        {
            var existingNames = await _db.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync();
            var missingNames = tagNames.Except(existingNames.Select(t => t.Name)).ToList();

            foreach (var name in missingNames)
            {
                var tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Slug = name.ToLowerInvariant().Replace(" ", "-")
                };
                _db.Tags.Add(tag);
                tagsToLink.Add(tag);
            }
            tagsToLink.AddRange(existingNames.Where(t => !tagsToLink.Any(x => x.Id == t.Id)));
        }

        foreach (var tag in tagsToLink.DistinctBy(t => t.Id))
        {
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
        }
    }

    private async Task SyncPostCategoriesAsync(Post post, List<Guid>? categoryIds)
    {
        if (categoryIds == null) return;

        if (post.PostCategories.Count != 0)
        {
            _db.Set<PostCategory>().RemoveRange(post.PostCategories);
            post.PostCategories.Clear();
        }

        if (categoryIds.Count == 0) return;

        var categories = await _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync();
        foreach (var category in categories)
        {
            post.PostCategories.Add(new PostCategory { PostId = post.Id, CategoryId = category.Id });
        }
    }

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string title)
    {
        var baseSlug = !string.IsNullOrWhiteSpace(providedSlug)
            ? providedSlug
            : title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
                .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c").Replace("œ", "oe").Replace("æ", "ae");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "article";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Posts.AnyAsync(p => p.Slug == candidate))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid entityId)
    {
        var candidate = slug;
        var suffix = 1;
        while (await _db.Posts.AnyAsync(p => p.Slug == candidate && p.Id != entityId))
        {
            candidate = $"{slug}-{suffix++}";
        }
        return candidate;
    }

    private static PostResponse MapToResponse(Post p) =>
        new(p.Id, p.Title, p.Slug, p.Content, p.Excerpt, p.CoverImageUrl,
            p.AuthorId, p.Status.ToString(), p.PublishedAt, p.CreatedAt, p.UpdatedAt,
            p.AuthorName, p.AuthorAvatar, p.PostType, p.ViewCount, [], []);
}
