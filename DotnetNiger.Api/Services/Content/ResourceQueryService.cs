using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de consultation des ressources (requêtes en lecture seule).</summary>
public class ResourceQueryService : IResourceQueryService
{
    private readonly DotnetNigerDbContext _db;

    public ResourceQueryService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste paginée des ressources avec filtres.</summary>
    public async Task<PaginatedResponse<ResourceResponse>> GetAllAsync(
        string? resourceType, string? level, string? query,
        string? tag, Guid? categoryId, int page, int pageSize, Guid? after = null, Guid? authorId = null)
    {
        var q = _db.Resources
            .Include(r => r.ResourceTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.ResourceCategories).ThenInclude(rc => rc.Category)
            .AsNoTracking();

        if (authorId.HasValue) q = q.Where(r => r.AuthorId == authorId.Value);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r => r.Title.Contains(query) || (r.Description != null && r.Description.Contains(query)));
        if (!string.IsNullOrWhiteSpace(resourceType))
            q = q.Where(r => r.ResourceType == resourceType);
        if (!string.IsNullOrWhiteSpace(level))
            q = q.Where(r => r.Level == level);
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(r => r.ResourceTags.Any(rt => rt.Tag.Slug == tag || rt.Tag.Name == tag));
        if (categoryId.HasValue)
            q = q.Where(r => r.ResourceCategories.Any(rc => rc.CategoryId == categoryId.Value));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ResourceResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Récupère une ressource par identifiant.</summary>
    public async Task<ResourceResponse?> GetByIdAsync(Guid id)
    {
        var r = await _db.Resources
            .Include(x => x.ResourceTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(x => x.Id == id);
        return r == null ? null : MapToResponse(r);
    }

    /// <summary>Récupère une ressource par slug.</summary>
    public async Task<ResourceResponse?> GetBySlugAsync(string slug)
    {
        var r = await _db.Resources
            .Include(x => x.ResourceTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(res => res.Slug == slug);
        return r == null ? null : MapToResponse(r);
    }

    /// <summary>Récupère la liste des types de ressources disponibles.</summary>
    public async Task<List<string>> GetResourceTypesAsync()
    {
        return await _db.Resources.AsNoTracking()
            .Where(r => r.ResourceType != null)
            .Select(r => r.ResourceType!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    /// <summary>Récupère la liste des niveaux de difficulté disponibles.</summary>
    public async Task<List<string>> GetLevelsAsync()
    {
        return await _db.Resources.AsNoTracking()
            .Where(r => r.Level != null)
            .Select(r => r.Level!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }

    private static ResourceResponse MapToResponse(Resource r) =>
        new(r.Id, r.Title, r.Slug, r.Description, r.Url, r.DownloadUrl, r.ThumbnailUrl,
            r.CreatedBy, r.Status.ToString(), r.ResourceType, r.Level, r.ViewCount,
            r.ResourceTags?.Select(rt => new TagResponse { Id = rt.Tag.Id, Name = rt.Tag.Name, Slug = rt.Tag.Slug }).ToList() ?? [],
            r.CreatedAt, r.UpdatedAt);
}
