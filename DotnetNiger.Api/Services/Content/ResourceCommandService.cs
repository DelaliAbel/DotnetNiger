using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de création, modification et suppression des ressources.</summary>
public class ResourceCommandService : IResourceCommandService
{
    private readonly DotnetNigerDbContext _db;

    public ResourceCommandService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée une nouvelle ressource avec ses tags et catégories.</summary>
    public async Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid authorId, bool isAdmin, bool isCollaborator)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Slug ?? string.Empty,
            Description = request.Description ?? string.Empty,
            Url = request.Url ?? string.Empty,
            DownloadUrl = request.DownloadUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            AuthorId = authorId,
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
        if (request.Slug != null) resource.Slug = request.Slug;
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

    /// <summary>Supprime une ressource (auteur ou admin uniquement).</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return false;
        if (!isAdmin && resource.AuthorId != userId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cette ressource.");
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Incrémente le compteur de vues d'une ressource.</summary>
    public async Task<ResourceResponse?> IncrementViewCountAsync(Guid id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return null;
        resource.ViewCount++;
        resource.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(resource);
    }

    /// <summary>Soumet une ressource pour modération.</summary>
    public async Task SubmitForReviewAsync(Guid id)
    {
        var resource = await _db.Resources.FindAsync(id)
            ?? throw new KeyNotFoundException("Ressource non trouvée");
        resource.Status = ResourceStatus.PendingReview;
        resource.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>Publie une ressource.</summary>
    public async Task PublishAsync(Guid id)
    {
        var resource = await _db.Resources.FindAsync(id)
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
            r.AuthorId, r.Status.ToString(), r.CreatedAt, r.UpdatedAt);
}
