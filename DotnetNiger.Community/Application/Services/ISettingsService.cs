using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

public interface ISettingsService
{
    Task<List<SiteSettingResponse>> GetAllAsync();
    Task<SiteSettingResponse?> GetByKeyAsync(string key);
    Task<SiteSettingResponse> SetAsync(string key, string value, string type = "string", string? description = null);
    Task SetBatchAsync(Dictionary<string, string> settings);
    Task<bool> DeleteAsync(string key);
}
