using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Services d'écriture pour les ressources pédagogiques (création, modification, suppression).</summary>
public class ResourceCommandService(AppDbContext db, ICertificateService certificateService) : IResourceCommandService
{
    /// <summary>Crée une ressource avec ses catégories et tags associés.</summary>
    public async Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid userId, bool isAdmin, bool isCollaborator)
    {
        var (canCreate, _, error) = await certificateService.CanCreateContentAsync(userId, isAdmin, isCollaborator);
        if (!canCreate)
        {
            if (error != null) throw new InvalidOperationException(error);
            throw new UnauthorizedAccessException();
        }
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = SlugGenerator.GenerateSlug(request.Title),
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Level = request.Level,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Resources.Add(resource);

        if (request.CategoryIds?.Count > 0)
        {
            foreach (var catId in request.CategoryIds)
            {
                db.Set<ResourceCategory>().Add(new ResourceCategory { ResourceId = resource.Id, CategoryId = catId });
            }
        }

        await AssignTags(resource, request.TagNames);
        await db.SaveChangesAsync();
        return ResourceMappers.ToResponse(resource);
    }

    /// <summary>Modifie une ressource (reconstruit les catégories et tags).</summary>
    public async Task<ResourceResponse?> UpdateAsync(Guid id, CreateResourceRequest request, Guid userId, bool isAdmin)
    {
        var r = await db.Resources
            .Include(r => r.ResourceTags)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (r is null) return null;
        if (r.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Resource.NotAuthorizedModify);

        r.Title = request.Title;
        r.Slug = SlugGenerator.GenerateSlug(request.Title);
        r.Description = request.Description;
        r.Url = request.Url;
        r.ResourceType = request.ResourceType;
        r.Level = request.Level;
        r.UpdatedAt = DateTime.UtcNow;

        var existingCategories = await db.Set<ResourceCategory>().Where(rc => rc.ResourceId == id).ToListAsync();
        db.Set<ResourceCategory>().RemoveRange(existingCategories);

        if (request.CategoryIds?.Count > 0)
        {
            foreach (var catId in request.CategoryIds)
            {
                db.Set<ResourceCategory>().Add(new ResourceCategory { ResourceId = id, CategoryId = catId });
            }
        }

        db.ResourceTags.RemoveRange(r.ResourceTags);
        await AssignTags(r, request.TagNames);
        await db.SaveChangesAsync();
        return ResourceMappers.ToResponse(r);
    }

    /// <summary>Suppression logique d'une ressource.</summary>
    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var r = await db.Resources.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
        if (r is null) return false;
        if (r.CreatedBy != userId && !isAdmin)
            throw new UnauthorizedAccessException(Messages.Resource.NotAuthorizedDelete);
        r.IsDeleted = true;
        r.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Incémente le compteur de vues de la ressource.</summary>
    public async Task<ResourceResponse?> IncrementViewCountAsync(Guid id)
    {
        var r = await db.Resources.FindAsync(id);
        if (r is null) return null;
        r.ViewCount++;
        await db.SaveChangesAsync();
        return ResourceMappers.ToResponse(r);
    }

    private async Task AssignTags(Resource resource, List<string> tagNames)
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
            resource.ResourceTags.Add(new ResourceTag { ResourceId = resource.Id, TagId = tag.Id });
        }
    }
}
