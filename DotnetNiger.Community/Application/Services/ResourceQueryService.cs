using System.Linq.Expressions;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Services de lecture pour les ressources pédagogiques (recherche, filtres, pagination).</summary>
public class ResourceQueryService(AppDbContext db) : IResourceQueryService
{
    /// <summary>Recherche paginée avec filtres (type, niveau, tag, catégorie, mot-clé). Supporte le curseur (after).</summary>
    public async Task<PaginatedResponse<ResourceResponse>> GetAllAsync(string? resourceType, string? level, string? query, string? tag, Guid? categoryId, int page = 1, int pageSize = 10, Guid? after = null, Guid? createdBy = null)
    {
        var q = BuildBaseQuery(resourceType, level, query, tag, categoryId, createdBy);

        List<ResourceResponse> items;
        int total;

        if (after.HasValue)
        {
            items = await q
                .Where(r => r.Id > after.Value)
                .OrderBy(r => r.Id)
                .Take(pageSize)
                .Select(Projection)
                .ToListAsync();
            total = items.Count;
        }
        else
        {
            total = await q.CountAsync();
            items = await q
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Projection)
                .ToListAsync();
        }

        return new PaginatedResponse<ResourceResponse> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    private IQueryable<Resource> BuildBaseQuery(string? resourceType, string? level, string? query, string? tag, Guid? categoryId, Guid? createdBy = null)
    {
        var q = db.Resources
            .AsNoTracking()
            .Include(r => r.ResourceCategories)
            .Include(r => r.ResourceTags).ThenInclude(rt => rt.Tag)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(resourceType)) q = q.Where(r => r.ResourceType == resourceType);
        if (!string.IsNullOrWhiteSpace(level)) q = q.Where(r => r.Level == level);
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(r => r.ResourceTags.Any(rt => rt.Tag.Slug == tag));
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r => r.Title.Contains(query) || r.Description.Contains(query));
        if (categoryId.HasValue)
            q = q.Where(r => r.ResourceCategories.Any(rc => rc.CategoryId == categoryId.Value));
        if (createdBy.HasValue) q = q.Where(r => r.CreatedBy == createdBy.Value);

        return q;
    }

    private static readonly Expression<Func<Resource, ResourceResponse>> Projection = r => new()
    {
        Id = r.Id,
        Title = r.Title,
        Slug = r.Slug,
        Description = r.Description,
        Url = r.Url,
        ResourceType = r.ResourceType,
        Level = r.Level,
        CreatedBy = r.CreatedBy,
        ViewCount = r.ViewCount,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        CategoryIds = r.ResourceCategories.Select(rc => rc.CategoryId).ToList(),
        Tags = r.ResourceTags.Select(rt => new TagResponse
        {
            Id = rt.Tag.Id,
            Name = rt.Tag.Name,
            Slug = rt.Tag.Slug,
            UsageCount = rt.Tag.UsageCount
        }).ToList()
    };

    /// <summary>Détail d'une ressource avec ses tags.</summary>
    public async Task<ResourceResponse?> GetByIdAsync(Guid id)
    {
        var r = await db.Resources
            .AsNoTracking()
            .Include(r => r.ResourceCategories)
            .Include(r => r.ResourceTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(r => r.Id == id);
        return r is null ? null : ResourceMappers.ToResponse(r);
    }

    /// <summary>Détail d'une ressource par son slug.</summary>
    public async Task<ResourceResponse?> GetBySlugAsync(string slug)
    {
        var r = await db.Resources
            .AsNoTracking()
            .Include(r => r.ResourceCategories)
            .Include(r => r.ResourceTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(r => r.Slug == slug);
        return r is null ? null : ResourceMappers.ToResponse(r);
    }

    /// <summary>Liste des types de ressources distincts.</summary>
    public async Task<List<string>> GetResourceTypesAsync()
    {
        return await db.Resources
            .AsNoTracking()
            .Select(r => r.ResourceType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    /// <summary>Liste des niveaux disponibles.</summary>
    public async Task<List<string>> GetLevelsAsync()
    {
        return await db.Resources
            .AsNoTracking()
            .Select(r => r.Level)
            .Distinct()
            .ToListAsync();
    }
}
