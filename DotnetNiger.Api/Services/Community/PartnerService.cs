using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Community;

/// <summary>Service de gestion des partenaires de la communauté.</summary>
public class PartnerService : IPartnerService
{
    private readonly DotnetNigerDbContext _db;

    public PartnerService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère les partenaires actifs, filtrés par type optionnel.</summary>
    public async Task<List<PartnerResponse>> GetAllActiveAsync(string? partnerType)
    {
        var q = _db.Set<Partner>().AsNoTracking().Where(p => p.IsActive && !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(partnerType))
            q = q.Where(p => p.PartnerType == partnerType);
        var partners = await q.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync();
        return partners.Select(MapToResponse).ToList();
    }

    /// <summary>Récupère un partenaire par identifiant.</summary>
    public async Task<PartnerResponse?> GetByIdAsync(Guid id)
    {
        var p = await _db.Set<Partner>().FindAsync(id);
        return p == null || p.IsDeleted ? null : MapToResponse(p);
    }

    /// <summary>Crée un nouveau partenaire.</summary>
    public async Task<PartnerResponse> CreateAsync(CreatePartnerRequest request)
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = await GenerateUniqueSlug(null, request.Name),
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
            PartnerType = request.PartnerType,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        _db.Set<Partner>().Add(partner);
        await _db.SaveChangesAsync();
        return MapToResponse(partner);
    }

    /// <summary>Met à jour les informations d'un partenaire.</summary>
    public async Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request)
    {
        var partner = await _db.Set<Partner>().FindAsync(id);
        if (partner == null || partner.IsDeleted) return null;
        partner.Name = request.Name;
        partner.Slug = await GenerateUniqueSlug(null, request.Name);
        partner.Description = request.Description;
        partner.LogoUrl = request.LogoUrl;
        partner.WebsiteUrl = request.WebsiteUrl;
        partner.PartnerType = request.PartnerType;
        partner.SortOrder = request.SortOrder;
        partner.IsActive = request.IsActive;
        partner.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToResponse(partner);
    }

    /// <summary>Supprime un partenaire (suppression logique).</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var partner = await _db.Set<Partner>().FindAsync(id);
        if (partner == null || partner.IsDeleted) return false;
        partner.IsDeleted = true;
        partner.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static PartnerResponse MapToResponse(Partner p) => new()
    {
        Id = p.Id, Name = p.Name, Slug = p.Slug, Description = p.Description,
        LogoUrl = p.LogoUrl, WebsiteUrl = p.WebsiteUrl, PartnerType = p.PartnerType,
        SortOrder = p.SortOrder, IsActive = p.IsActive, CreatedAt = p.CreatedAt
    };

    private async Task<string> GenerateUniqueSlug(string? providedSlug, string name)
    {
        var baseSlug = !string.IsNullOrWhiteSpace(providedSlug)
            ? providedSlug
            : name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("î", "i").Replace("ï", "i")
                .Replace("ô", "o").Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ç", "c");

        baseSlug = new string(baseSlug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        baseSlug = baseSlug.Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "partenaire";

        var candidate = baseSlug;
        var suffix = 1;
        while (await _db.Set<Partner>().AnyAsync(p => p.Slug == candidate && !p.IsDeleted))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }
        return candidate;
    }
}
