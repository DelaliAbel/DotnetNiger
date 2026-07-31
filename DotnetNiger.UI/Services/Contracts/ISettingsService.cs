using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface ISettingsService
{
    Task<List<SiteSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SiteSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<SiteSettingDto?> SetAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<bool> SetBatchAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<PublicSettingsResponse?> GetPublicSettingsAsync();
}
