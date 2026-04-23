using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IAppSettingPersistence : ICrudPersistence<AppSetting>
{
    Task<AppSetting?> GetByKeyAsync(string key);
    Task<string?> GetValueAsync(string key);
    Task<AppSetting> SetValueAsync(string key, string value, Guid? updatedByUserId = null);
}
