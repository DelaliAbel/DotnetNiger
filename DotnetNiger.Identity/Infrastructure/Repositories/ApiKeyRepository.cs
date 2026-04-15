using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Infrastructure.Repositories;

// Repository pour les cles API.
public class ApiKeyRepository : BaseRepository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(DotnetNigerIdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<ApiKey?> GetByIdAndUserAsync(Guid apiKeyId, Guid userId)
    {
        return DbContext.ApiKeys.FirstOrDefaultAsync(key => key.Id == apiKeyId && key.UserId == userId);
    }

    public Task<ApiKey?> GetByIdWithUserAsync(Guid apiKeyId)
    {
        return DbContext.ApiKeys
            .Include(key => key.User)
            .FirstOrDefaultAsync(key => key.Id == apiKeyId);
    }

    public async Task<IReadOnlyList<ApiKey>> GetByUserAsync(Guid userId)
    {
        return await DbContext.ApiKeys
            .AsNoTracking()
            .Where(key => key.UserId == userId)
            .OrderByDescending(key => key.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ApiKey>> GetActiveByUserAsync(Guid userId)
    {
        return await DbContext.ApiKeys
            .Where(key => key.UserId == userId && key.IsActive)
            .ToListAsync();
    }

    public IQueryable<ApiKey> QueryWithUser()
    {
        return DbContext.ApiKeys
            .AsNoTracking()
            .Include(key => key.User)
            .AsQueryable();
    }
}
