using DotnetNiger.Common.Extensions;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des tags (étiquettes attachées aux articles, événements, ressources).</summary>
public class TagService(AppDbContext db) : ITagService
{
    /// <summary>Liste tous les tags triés par nom.</summary>
    public async Task<List<TagResponse>> GetAllAsync()
    {
        var tags = await db.Tags.AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
        return tags.Select(MapTag).ToList();
    }

    /// <summary>Détail d'un tag par son identifiant.</summary>
    public async Task<TagResponse?> GetByIdAsync(Guid id)
    {
        var t = await db.Tags.FindAsync(id);
        return t is null ? null : MapTag(t);
    }

    /// <summary>Détail d'un tag par son slug.</summary>
    public async Task<TagResponse?> GetBySlugAsync(string slug)
    {
        var t = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
        return t is null ? null : MapTag(t);
    }

    /// <summary>Crée un tag avec un slug généré automatiquement.</summary>
    public async Task<TagResponse> CreateAsync(string name)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = SlugGenerator.GenerateSlug(name)
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return MapTag(tag);
    }

    /// <summary>Modifie un tag (nom et slug recalculé).</summary>
    public async Task<TagResponse?> UpdateAsync(Guid id, string name)
    {
        var t = await db.Tags.FindAsync(id);
        if (t is null) return null;

        t.Name = name;
        t.Slug = SlugGenerator.GenerateSlug(name);
        await db.SaveChangesAsync();
        return MapTag(t);
    }

    /// <summary>Supprime un tag.</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var t = await db.Tags.FindAsync(id);
        if (t is null) return false;

        db.Tags.Remove(t);
        await db.SaveChangesAsync();
        return true;
    }

    private static TagResponse MapTag(Tag t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Slug = t.Slug,
        UsageCount = t.UsageCount
    };
}
