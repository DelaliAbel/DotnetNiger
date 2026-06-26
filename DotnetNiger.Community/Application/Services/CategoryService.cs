using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des catégories de contenu.</summary>
public class CategoryService(AppDbContext db) : ICategoryService
{
    /// <summary>Liste toutes les catégories triées par nom.</summary>
    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
        return categories.Select(MapCategory).ToList();
    }

    /// <summary>Détail d'une catégorie par son identifiant.</summary>
    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var c = await db.Categories.FindAsync(id);
        return c is null ? null : MapCategory(c);
    }

    /// <summary>Détail d'une catégorie par son slug.</summary>
    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        var c = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);
        return c is null ? null : MapCategory(c);
    }

    /// <summary>Crée une catégorie avec un slug généré automatiquement.</summary>
    public async Task<CategoryResponse> CreateAsync(string name, string description)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = SlugGenerator.Generate(name),
            Description = description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return MapCategory(category);
    }

    /// <summary>Modifie le nom, le slug et la description d'une catégorie.</summary>
    public async Task<CategoryResponse?> UpdateAsync(Guid id, string name, string description)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return null;

        c.Name = name;
        c.Slug = SlugGenerator.Generate(name);
        c.Description = description;
        await db.SaveChangesAsync();
        return MapCategory(c);
    }

    /// <summary>Supprime une catégorie.</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return false;

        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return true;
    }

    private static CategoryResponse MapCategory(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        PostCount = c.PostCount
    };
}
