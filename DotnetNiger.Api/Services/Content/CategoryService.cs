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
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    /// <summary>Récupère toutes les catégories.</summary>
    public async Task<PaginatedResponse<CategoryResponse>> GetAllAsync()
    {
        var query = _db.Categories.AsNoTracking();
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
        return category == null ? null : MapToResponse(category);
    }

    /// <summary>Récupère une catégorie par slug.</summary>
    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        return category == null ? null : MapToResponse(category);
    }

    /// <summary>Met à jour le nom et la description d'une catégorie.</summary>
    public async Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = name;
        category.Description = description ?? string.Empty;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    /// <summary>Supprime une catégorie.</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    private static CategoryResponse MapToResponse(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.IconUrl);
}
