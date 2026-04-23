using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Domain.Persistence;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Persistence;

public class CommunitySettingPersistence : ICommunitySettingPersistence
{
    private readonly CommunityDbContext _context;

    public CommunitySettingPersistence(CommunityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<AppSetting> UpsertAsync(string key, string value, string? description, string dataType, bool isPublic, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AppSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (existing is null)
        {
            var setting = new AppSetting
            {
                Key = key,
                Value = value,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.AppSettings.Add(setting);
            await _context.SaveChangesAsync(cancellationToken);
            return setting;
        }

        existing.Value = value;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
