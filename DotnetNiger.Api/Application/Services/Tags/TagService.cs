using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Tags;

/// <summary>Service de gestion des tags pour le contenu.</summary>
public class TagService : ITagService
{
    private readonly DotnetNigerDbContext _db;

    public TagService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère la liste de tous les tags.</summary>
    public async Task<List<TagResponse>> GetAllAsync(CancellationToken ct = default)
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
            .ToListAsync(ct);
    }

    /// <summary>Récupère un tag par identifiant.</summary>
    public async Task<TagResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FindAsync(id, ct);
        return tag == null ? null : MapToResponse(tag);
    }

    /// <summary>Récupère un tag par slug.</summary>
    public async Task<TagResponse?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct);
        return tag == null ? null : MapToResponse(tag);
    }

    /// <summary>Crée un nouveau tag.</summary>
    public async Task<TagResponse> CreateAsync(string name, CancellationToken ct = default)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = await GenerateUniqueSlug(name, ct)
        };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(tag);
    }

    /// <summary>Met à jour le nom d'un tag.</summary>
    public async Task<TagResponse?> UpdateAsync(Guid id, string name, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag == null) return null;
        tag.Name = name;
        tag.Slug = await EnsureUniqueSlug(name, id, ct);
        await _db.SaveChangesAsync(ct);
        return MapToResponse(tag);
    }

    /// <summary>Supprime un tag (suppression définitive).</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag == null) return false;
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static TagResponse MapToResponse(Tag t) => new()
    {
        Id = t.Id, Name = t.Name, Slug = t.Slug, UsageCount = t.UsageCount
    };

    private async Task<string> GenerateUniqueSlug(string name, CancellationToken ct = default)
    {
        var baseSlug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
            .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "tag";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Tags.AnyAsync(t => t.Slug == candidate, ct))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }

    private async Task<string> EnsureUniqueSlug(string name, Guid entityId, CancellationToken ct = default)
    {
        var baseSlug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
            .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
            .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
            .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "tag";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Tags.AnyAsync(t => t.Slug == candidate && t.Id != entityId, ct))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }
}
