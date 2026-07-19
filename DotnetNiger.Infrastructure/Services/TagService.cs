using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly DotnetNigerDbContext _db;

    public TagService(DotnetNigerDbContext db) => _db = db;

    public async Task<List<TagResponse>> GetAllAsync()
    {
        return await _db.Tags.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                UsageCount = t.UsageCount
            })
            .ToListAsync();
    }

    public async Task<TagResponse?> GetByIdAsync(Guid id)
    {
        var tag = await _db.Tags.FindAsync(id);
        return tag == null ? null : MapToResponse(tag);
    }

    public async Task<TagResponse?> GetBySlugAsync(string slug)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
        return tag == null ? null : MapToResponse(tag);
    }

    public async Task<TagResponse> CreateAsync(string name)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLower().Replace(" ", "-")
        };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();
        return MapToResponse(tag);
    }

    public async Task<TagResponse?> UpdateAsync(Guid id, string name)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return null;
        tag.Name = name;
        tag.Slug = name.ToLower().Replace(" ", "-");
        await _db.SaveChangesAsync();
        return MapToResponse(tag);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return false;
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TagResponse MapToResponse(Tag t) => new()
    {
        Id = t.Id, Name = t.Name, Slug = t.Slug, UsageCount = t.UsageCount
    };
}
