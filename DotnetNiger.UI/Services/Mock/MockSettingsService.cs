using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockSettingsService : ISettingsService
{
    private readonly List<SiteSettingDto> _settings = new()
    {
        new SiteSettingDto { Key = "site.name", Value = "DotnetNiger", Type = "string", Description = "Nom du site" },
        new SiteSettingDto { Key = "site.description", Value = "Communauté .NET du Niger", Type = "string", Description = "Description du site" },
        new SiteSettingDto { Key = "site.theme", Value = "light", Type = "string", Description = "Thème par défaut" }
    };

    public Task<List<SiteSettingDto>> GetAllAsync() => Task.FromResult(_settings.ToList());

    public Task<SiteSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_settings.FirstOrDefault(s => s.Key == key));

    public Task<SiteSettingDto?> SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = _settings.FirstOrDefault(s => s.Key == key);
        if (setting is null)
        {
            setting = new SiteSettingDto { Key = key, Value = value };
            _settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }
        return Task.FromResult<SiteSettingDto?>(setting);
    }

    public Task<bool> SetBatchAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
    {
        foreach (var kv in settings)
        {
            var existing = _settings.FirstOrDefault(s => s.Key == kv.Key);
            if (existing is not null)
                existing.Value = kv.Value;
            else
                _settings.Add(new SiteSettingDto { Key = kv.Key, Value = kv.Value });
        }
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var removed = _settings.RemoveAll(s => s.Key == key);
        return Task.FromResult(removed > 0);
    }

    public Task<PublicSettingsResponse?> GetPublicSettingsAsync()
    {
        var dict = _settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);
        return Task.FromResult<PublicSettingsResponse?>(new PublicSettingsResponse
        {
            SiteName = dict.GetValueOrDefault("site_name", ".NET Niger"),
            LogoNom = dict.GetValueOrDefault("logo_nom", ".NET Niger"),
            LogoUrl = dict.GetValueOrDefault("logo_url", ""),
            ContactEmail = dict.GetValueOrDefault("contact_email", ""),
            Tel = dict.GetValueOrDefault("tel", ""),
            Location = dict.GetValueOrDefault("location", ""),
            FacebookUrl = dict.GetValueOrDefault("facebook_url", ""),
            LinkedInUrl = dict.GetValueOrDefault("linkedin_url", ""),
            WhatsAppUrl = dict.GetValueOrDefault("whatsapp_url", ""),
            YoutubeUrl = dict.GetValueOrDefault("youtube_url", "")
        });
    }
}
