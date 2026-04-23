using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Domain.Persistence;

public interface ICommunitySettingPersistence
{
    Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<AppSetting> UpsertAsync(string key, string value, string? description, string dataType, bool isPublic, CancellationToken cancellationToken = default);
}
