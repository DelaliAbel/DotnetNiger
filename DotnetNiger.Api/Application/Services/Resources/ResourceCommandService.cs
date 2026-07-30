using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Resources;

/// <summary>Service de création, modification et suppression des ressources.</summary>
public class ResourceCommandService : IResourceCommandService
{
    private readonly DotnetNigerDbContext _db;

    public ResourceCommandService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée une nouvelle ressource avec ses tags et catégories.</summary>
    public async Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid authorId, bool isAdmin, bool isCollaborator)
    {
        var slug = await GenerateUniqueSlug(request.Slug, request.Title);

        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Description = request.Description ?? string.Empty,
            Url = request.Url ?? string.Empty,
            DownloadUrl = request.DownloadUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            ResourceType = request.ResourceType ?? string.Empty,
            Level = request.Level ?? string.Empty,
            AuthorId = authorId,
            CreatedBy = authorId,
            Status = isAdmin || isCollaborator ? ResourceStatus.Published : ResourceStatus.Draft
        };

        await SyncResourceTagsAsync(resource, request.TagNames, request.TagIds);
        await SyncResourceCategoriesAsync(resource, request.CategoryIds);

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        return MapToResponse(resource);
    }

    /// <summary>Met à jour une ressource existante.</summary>
    public async Task<ResourceResponse?> UpdateAsync(Guid id, UpdateResourceRequest request, Guid userId, bool isAdmin)
    {
        var resource = await _db.Resources
            .Include(r => r.ResourceTags)
            .Include(r => r.ResourceCategories)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) return null;

        if (!isAdmin && resource.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cette ressource.");

        if (request.Title != null) resource.Title = request.Title;
        if (request.Slug != null) resource.Slug = await EnsureUniqueSlug(request.Slug, resource.Id);
        if (request.Description != null) resource.Description = request.Description;
        if (request.Url != null) resource.Url = request.Url;
        if (request.DownloadUrl != null) resource.DownloadUrl = request.DownloadUrl;
        if (request.ThumbnailUrl != null) resource.ThumbnailUrl = request.ThumbnailUrl;
        if (request.ResourceType != null) resource.ResourceType = request.ResourceType;
        if (request.Level != null) resource.Level = request.Level;

        if (request.TagNames != null)
            await SyncResourceTagsAsync(resource, request.TagNames, request.TagIds);
        if (request.CategoryIds != null)
            await SyncResourceCategoriesAsync(resource, request.CategoryIds);

        resource.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(resource);
    }

    /// <summary>Supprime une ressource de façon permanente.</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) return false;
        if (!isAdmin && resource.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cette ressource.");

        var resourceId = resource.Id;
        var tags = await _db.ResourceTags.Where(t => t.ResourceId == resourceId).ToListAsync();
        _db.ResourceTags.RemoveRange(tags);
        var categories = await _db.ResourceCategories.Where(c => c.ResourceId == resourceId).ToListAsync();
        _db.ResourceCategories.RemoveRange(categories);

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Incrémente le compteur de vues d'une ressource.</summary>
    public async Task<ResourceResponse?> IncrementViewCountAsync(Guid id)
    {
        var affected = await _db.Resources
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.ViewCount, r => r.ViewCount + 1)
                .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));

        if (affected == 0) return null;

        var resource = await _db.Resources.FindAsync(id);
        return resource == null ? null : MapToResponse(resource);
    }

    /// <summary>Soumet une ressource pour modération.</summary>
    public async Task SubmitForReviewAsync(Guid id)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Ressource non trouvée");
        resource.Status = ResourceStatus.PendingReview;
        resource.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Publie une ressource.</summary>
    public async Task PublishAsync(Guid id)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Ressource non trouvée");
        resource.Status = ResourceStatus.Published;
        resource.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task SyncResourceTagsAsync(Resource resource, List<string>? tagNames, List<Guid>? tagIds)
    {
        if (resource.ResourceTags.Count != 0)
        {
            _db.Set<ResourceTag>().RemoveRange(resource.ResourceTags);
            resource.ResourceTags.Clear();
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
            resource.ResourceTags.Add(new ResourceTag { ResourceId = resource.Id, TagId = tag.Id });
        }
    }

    private async Task SyncResourceCategoriesAsync(Resource resource, List<Guid>? categoryIds)
    {
        if (categoryIds == null) return;

        if (resource.ResourceCategories.Count != 0)
        {
            _db.Set<ResourceCategory>().RemoveRange(resource.ResourceCategories);
            resource.ResourceCategories.Clear();
        }

        if (categoryIds.Count == 0) return;

        var categories = await _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync();
        foreach (var category in categories)
        {
            resource.ResourceCategories.Add(new ResourceCategory { ResourceId = resource.Id, CategoryId = category.Id });
        }
    }

    private static ResourceResponse MapToResponse(Resource r) =>
        new(r.Id, r.Title, r.Slug, r.Description, r.Url, r.DownloadUrl, r.ThumbnailUrl,
            r.CreatedBy, r.Status.ToString(), r.ResourceType, r.Level, r.ViewCount,
            r.ResourceTags?.Select(rt => new TagResponse { Id = rt.Tag.Id, Name = rt.Tag.Name, Slug = rt.Tag.Slug }).ToList() ?? [],
            r.CreatedAt, r.UpdatedAt);

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string title)
    {
        var baseSlug = !string.IsNullOrWhiteSpace(providedSlug)
            ? providedSlug
            : title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
                .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "ressource";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Resources.AnyAsync(r => r.Slug == candidate))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string slug, Guid entityId)
    {
        var candidate = slug;
        var suffix = 1;
        while (await _db.Resources.AnyAsync(r => r.Slug == candidate && r.Id != entityId))
        {
            candidate = $"{slug}-{suffix++}";
        }
        return candidate;
    }
}
