using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class SettingsService(AppDbContext db) : ISettingsService
{
    public async Task<List<SiteSettingDto>> GetAllAsync()
    {
        return await db.SiteSettings
            .OrderBy(s => s.Key)
            .Select(s => new SiteSettingDto
            {
                Key = s.Key,
                Value = s.Value,
                Type = s.Type,
                Description = s.Description
            })
            .ToListAsync();
    }

    public async Task<SiteSettingDto?> GetByKeyAsync(string key)
    {
        var setting = await db.SiteSettings.FindAsync(key);
        return setting is null ? null : Map(setting);
    }

    public async Task<SiteSettingDto> SetAsync(string key, string value, string type = "string", string? description = null)
    {
        var setting = await db.SiteSettings.FindAsync(key);
        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = key,
                Value = value,
                Type = type,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            };
            db.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            if (description is not null) setting.Description = description;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Map(setting);
    }

    public async Task SetBatchAsync(Dictionary<string, string> settings)
    {
        var keys = settings.Keys.ToList();
        var existing = await db.SiteSettings
            .Where(s => keys.Contains(s.Key))
            .ToListAsync();

        var existingByKey = existing.ToDictionary(s => s.Key);

        foreach (var (key, value) in settings)
        {
            if (existingByKey.TryGetValue(key, out var setting))
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                db.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var setting = await db.SiteSettings.FindAsync(key);
        if (setting is null) return false;
        db.SiteSettings.Remove(setting);
        await db.SaveChangesAsync();
        return true;
    }

    private static SiteSettingDto Map(SiteSetting s) => new()
    {
        Key = s.Key,
        Value = s.Value,
        Type = s.Type,
        Description = s.Description
    };
}
