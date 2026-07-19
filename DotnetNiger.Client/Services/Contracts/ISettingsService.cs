using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface ISettingsService
{
    Task<List<SiteSettingDto>> GetAllAsync();
    Task<SiteSettingDto?> GetByKeyAsync(string key);
    Task<SiteSettingDto?> SetAsync(string key, string value);
    Task<bool> SetBatchAsync(Dictionary<string, string> settings);
    Task<bool> DeleteAsync(string key);
}
