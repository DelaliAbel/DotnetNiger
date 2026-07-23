using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

/// <summary>Service de gestion des paramètres du site (CRUD clé/valeur).</summary>
public class SettingsService : ISettingsService
{
    private readonly DotnetNigerDbContext _db;

    public SettingsService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Récupère tous les paramètres du site.</summary>
    public async Task<List<SiteSettingResponse>> GetAllAsync()
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
            .ToListAsync();
    }

    /// <summary>Récupère un paramètre par sa clé.</summary>
    public async Task<SiteSettingResponse?> GetByKeyAsync(string key)
    {
        var setting = await _db.SiteSettings.FindAsync(key);
        return setting == null ? null : MapToResponse(setting);
    }

    /// <summary>Définit ou met à jour un paramètre par clé/valeur.</summary>
    public async Task<SiteSettingResponse> SetAsync(string key, string value)
    {
        var setting = await _db.SiteSettings.FindAsync(key);
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
        await _db.SaveChangesAsync();
        return MapToResponse(setting);
    }

    /// <summary>Définit plusieurs paramètres en une seule opération.</summary>
    public async Task SetBatchAsync(Dictionary<string, string> settings)
    {
        foreach (var (key, value) in settings)
            await SetAsync(key, value);
    }

    /// <summary>Supprime un paramètre par sa clé.</summary>
    public async Task<bool> DeleteAsync(string key)
    {
        var setting = await _db.SiteSettings.FindAsync(key);
        if (setting == null) return false;
        _db.SiteSettings.Remove(setting);
        await _db.SaveChangesAsync();
        return true;
    }

    private static SiteSettingResponse MapToResponse(SiteSetting s) => new()
    {
        Key = s.Key, Value = s.Value, Type = s.Type, Description = s.Description
    };
}
