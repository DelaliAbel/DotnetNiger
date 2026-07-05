using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

public interface ISettingsService
{
    Task<List<SiteSettingDto>> GetAllAsync();
    Task<SiteSettingDto?> GetByKeyAsync(string key);
    Task<SiteSettingDto> SetAsync(string key, string value, string type = "string", string? description = null);
    Task SetBatchAsync(Dictionary<string, string> settings);
    Task<bool> DeleteAsync(string key);
}
