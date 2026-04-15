using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Repository pour les parametres applicatifs persists.
public class AppSettingRepository : BaseRepository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(DotnetNigerIdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public string? GetValue(string key)
    {
        return DbContext.AppSettings
            .AsNoTracking()
            .Where(item => item.Key == key)
            .Select(item => item.Value)
            .FirstOrDefault();
    }

    public IReadOnlyDictionary<string, string> GetByPrefix(string prefix)
    {
        return DbContext.AppSettings
            .AsNoTracking()
            .Where(item => item.Key.StartsWith(prefix))
            .ToDictionary(item => item.Key, item => item.Value);
    }

    public void SetValues(IReadOnlyDictionary<string, string> values, Guid? updatedByUserId = null)
    {
        if (values.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var keys = values.Keys.ToList();
        var existing = DbContext.AppSettings
            .Where(item => keys.Contains(item.Key))
            .ToDictionary(item => item.Key, item => item);

        foreach (var entry in values)
        {
            if (existing.TryGetValue(entry.Key, out var item))
            {
                item.Value = entry.Value;
                item.UpdatedAt = now;
                item.UpdatedByUserId = updatedByUserId;
            }
            else
            {
                DbContext.AppSettings.Add(new AppSetting
                {
                    Key = entry.Key,
                    Value = entry.Value,
                    UpdatedAt = now,
                    UpdatedByUserId = updatedByUserId
                });
            }
        }

        DbContext.SaveChanges();
    }
}
