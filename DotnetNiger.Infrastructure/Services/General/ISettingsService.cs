using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.General;

public interface ISettingsService
{
    Task<List<SiteSettingResponse>> GetAllAsync();
    Task<SiteSettingResponse?> GetByKeyAsync(string key);
    Task<SiteSettingResponse> SetAsync(string key, string value);
    Task SetBatchAsync(Dictionary<string, string> settings);
    Task<bool> DeleteAsync(string key);
}
