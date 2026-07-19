using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly DotnetNigerDbContext _db;

    public SettingsService(DotnetNigerDbContext db) => _db = db;

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

    public async Task<SiteSettingResponse?> GetByKeyAsync(string key)
    {
        var setting = await _db.SiteSettings.FindAsync(key);
        return setting == null ? null : MapToResponse(setting);
    }

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

    public async Task SetBatchAsync(Dictionary<string, string> settings)
    {
        foreach (var (key, value) in settings)
            await SetAsync(key, value);
    }

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
