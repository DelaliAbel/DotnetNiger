using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IAppSettingRepository : IRepository<AppSetting>, IAppSettingPersistence
{
}

public class AppSettingRepository : BaseRepository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(CommunityDbContext context) : base(context)
    {
    }

    public Task<AppSetting?> GetByKeyAsync(string key)
    {
        return _dbSet.AsNoTracking().FirstOrDefaultAsync(item => item.Key == key);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        return await _dbSet.AsNoTracking()
            .Where(item => item.Key == key)
            .Select(item => item.Value)
            .FirstOrDefaultAsync();
    }

    public async Task<AppSetting> SetValueAsync(string key, string value, Guid? updatedByUserId = null)
    {
        var setting = await _dbSet.FirstOrDefaultAsync(item => item.Key == key);
        if (setting == null)
        {
            setting = new AppSetting
            {
                Key = key,
                Value = value,
                UpdatedByUserId = updatedByUserId,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dbSet.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedByUserId = updatedByUserId;
            setting.UpdatedAtUtc = DateTime.UtcNow;
            _dbSet.Update(setting);
        }

        await _context.SaveChangesAsync();
        return setting;
    }
}
