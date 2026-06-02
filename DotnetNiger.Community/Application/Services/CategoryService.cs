using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => MapCategory(c))
            .ToListAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var c = await db.Categories.FindAsync(id);
        return c is null ? null : MapCategory(c);
    }

    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        var c = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        return c is null ? null : MapCategory(c);
    }

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
