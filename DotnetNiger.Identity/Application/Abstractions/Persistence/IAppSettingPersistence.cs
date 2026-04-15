using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Abstractions.Persistence;

public interface IAppSettingPersistence : IRepositoryPersistence<AppSetting>
{
    string? GetValue(string key);
    IReadOnlyDictionary<string, string> GetByPrefix(string prefix);
    void SetValues(IReadOnlyDictionary<string, string> values, Guid? updatedByUserId = null);
}
