using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface ISettingsService
{
    Task<List<SiteSettingDto>> GetAllAsync();
    Task<SiteSettingDto?> GetByKeyAsync(string key);
    Task<SiteSettingDto?> SetAsync(string key, string value);
    Task<bool> SetBatchAsync(Dictionary<string, string> settings);
    Task<bool> DeleteAsync(string key);
    Task<PublicSettingsResponse?> GetPublicSettingsAsync();
}
