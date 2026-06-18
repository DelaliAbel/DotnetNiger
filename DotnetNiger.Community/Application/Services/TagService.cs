using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Domain;
using DotnetNiger.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class TagService(AppDbContext db) : ITagService
{
    public async Task<List<TagResponse>> GetAllAsync()
    {
        var tags = await db.Tags.AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
        return tags.Select(MapTag).ToList();
    }

    public async Task<TagResponse?> GetByIdAsync(Guid id)
    {
        var t = await db.Tags.FindAsync(id);
        return t is null ? null : MapTag(t);
    }

    public async Task<TagResponse?> GetBySlugAsync(string slug)
    {
        var t = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
        return t is null ? null : MapTag(t);
    }

    public async Task<TagResponse> CreateAsync(string name)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = SlugGenerator.Generate(name)
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return MapTag(tag);
    }

    public async Task<TagResponse?> UpdateAsync(Guid id, string name)
    {
        var t = await db.Tags.FindAsync(id);
        if (t is null) return null;

        t.Name = name;
        t.Slug = SlugGenerator.Generate(name);
        await db.SaveChangesAsync();
        return MapTag(t);
    }

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
