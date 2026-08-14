using System.Threading;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Settings;

/// <summary>Service de gestion des paramètres du site (CRUD clé/valeur).</summary>
public class SettingsService : ISettingsService
{
    private readonly DotnetNigerDbContext _db;

    public SettingsService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère tous les paramètres du site.</summary>
    public async Task<List<SiteSettingResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.SiteSettings.AsNoTracking()
            .OrderBy(s => s.Key)
            .Select(s => new SiteSettingResponse
            {
                Key = s.Key,
                Value = s.Value,
                Type = s.Type,
                Description = s.Description
            })
            .ToListAsync(ct);
    }

    /// <summary>Récupère un paramètre par sa clé.</summary>
    public async Task<SiteSettingResponse?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var setting = await _db.SiteSettings.FindAsync(key, ct);
        return setting == null ? null : MapToResponse(setting);
    }

    /// <summary>Définit ou met à jour un paramètre par clé/valeur.</summary>
    public async Task<SiteSettingResponse> SetAsync(string key, string value, CancellationToken ct = default)
    {
        var setting = await _db.SiteSettings.FindAsync(key, ct);
        if (setting == null)
        {
            setting = new SiteSetting
            {
                Id = key,
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            };
            _db.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return MapToResponse(setting);
    }

    /// <summary>Définit plusieurs paramètres en une seule opération atomique.</summary>
    public async Task SetBatchAsync(Dictionary<string, string> settings, CancellationToken ct = default)
    {
        foreach (var (key, value) in settings)
        {
            var setting = await _db.SiteSettings.FindAsync(key, ct);
            if (setting == null)
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    Id = key,
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Récupère les paramètres publics du site.</summary>
    public async Task<PublicSettingsResponse> GetPublicSettingsAsync(CancellationToken ct = default)
    {
        var dict = await _db.SiteSettings.AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase, ct);

        return new PublicSettingsResponse
        {
            SiteName = dict.GetValueOrDefault("site_name", ".NET Niger"),
            DefaultOgImage = dict.GetValueOrDefault("default_og_image", "/images/og-default.jpg"),
            LogoNom = dict.GetValueOrDefault("logo_nom", ".NET Niger"),
            LogoUrl = dict.GetValueOrDefault("logo_url", ""),
            ContactEmail = dict.GetValueOrDefault("contact_email", ""),
            Tel = dict.GetValueOrDefault("tel", ""),
            Location = dict.GetValueOrDefault("location", ""),
            FacebookUrl = dict.GetValueOrDefault("facebook_url", ""),
            LinkedInUrl = dict.GetValueOrDefault("linkedin_url", ""),
            WhatsAppUrl = dict.GetValueOrDefault("whatsapp_url", ""),
            YoutubeUrl = dict.GetValueOrDefault("youtube_url", "")
        };
    }

    /// <summary>Supprime un paramètre par sa clé.</summary>
    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        var setting = await _db.SiteSettings.FindAsync(key, ct);
        if (setting == null) return false;
        _db.SiteSettings.Remove(setting);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static SiteSettingResponse MapToResponse(SiteSetting s) => new()
    {
        Key = s.Key, Value = s.Value, Type = s.Type, Description = s.Description
    };
}
