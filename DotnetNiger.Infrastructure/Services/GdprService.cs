using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

/// <summary>Gestion des consentements RGPD.</summary>
public class GdprService
{
    private readonly DotnetNigerDbContext _db;

    public GdprService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Enregistre un consentement utilisateur.</summary>
    public async Task RecordConsentAsync(Guid userId, string consentType,
        string consentVersion, bool granted, string? ipAddress, string? userAgent)
    {
        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConsentType = consentType,
            ConsentVersion = consentVersion,
            Granted = granted,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        _db.UserConsents.Add(consent);
        await _db.SaveChangesAsync();
    }

    /// <summary>Historique complet des consentements.</summary>
    public async Task<List<ConsentResponse>> GetConsentHistoryAsync(Guid userId) =>
        await _db.UserConsents.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConsentResponse(c.ConsentType, c.ConsentVersion, c.Granted, c.CreatedAt))
            .ToListAsync();

    /// <summary>Dernier consentement par type.</summary>
    public async Task<List<ConsentResponse>> GetLatestConsentsAsync(Guid userId) =>
        await _db.UserConsents.AsNoTracking()
            .Where(c => c.UserId == userId)
            .GroupBy(c => c.ConsentType)
            .Select(g => g.OrderByDescending(c => c.CreatedAt).First())
            .Select(c => new ConsentResponse(c.ConsentType, c.ConsentVersion, c.Granted, c.CreatedAt))
            .ToListAsync();
}
