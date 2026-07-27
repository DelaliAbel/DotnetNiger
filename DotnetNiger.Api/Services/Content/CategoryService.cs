using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Service de gestion des catégories de contenu.</summary>
public class CategoryService : ICategoryService
{
    private readonly DotnetNigerDbContext _db;

    public CategoryService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Crée une nouvelle catégorie.</summary>
    public async Task<CategoryResponse> CreateAsync(string name, string? description)
    {
        var slug = await GenerateUniqueSlug(name);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description ?? string.Empty
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    /// <summary>Récupère toutes les catégories.</summary>
    public async Task<PaginatedResponse<CategoryResponse>> GetAllAsync()
    {
        var query = _db.Categories.AsNoTracking().Where(c => !c.IsDeleted);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return new PaginatedResponse<CategoryResponse>(
            items.Select(MapToResponse).ToList(), totalCount, 1, totalCount);
    }

    /// <summary>Récupère une catégorie par identifiant.</summary>
    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category == null || category.IsDeleted ? null : MapToResponse(category);
    }

    /// <summary>Récupère une catégorie par slug.</summary>
    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        return category == null ? null : MapToResponse(category);
    }

    /// <summary>Met à jour le nom et la description d'une catégorie.</summary>
    public async Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category == null) return null;

        category.Name = name;
        category.Slug = await EnsureUniqueSlug(name, id);
        category.Description = description ?? string.Empty;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    /// <summary>Supprime une catégorie (suppression logique).</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category == null) return false;
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static CategoryResponse MapToResponse(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.IconUrl);

    private async Task<string> GenerateUniqueSlug(string name)
    {
        var baseSlug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
            .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "categorie";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Categories.AnyAsync(c => c.Slug == candidate && !c.IsDeleted))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string name, Guid entityId)
    {
        var baseSlug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
            .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "categorie";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Categories.AnyAsync(c => c.Slug == candidate && c.Id != entityId && !c.IsDeleted))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }
}
