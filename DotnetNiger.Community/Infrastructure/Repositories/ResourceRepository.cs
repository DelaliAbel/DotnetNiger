using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IResourceRepository : IRepository<Resource>
{
    Task<Resource?> GetBySlugAsync(string slug);
    Task<IEnumerable<Resource>> GetByCategoryAsync(Guid categoryId);
}

public class ResourceRepository : BaseRepository<Resource>, IResourceRepository
{
    public ResourceRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Resource?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(r => r.ResourceCategories)
            .FirstOrDefaultAsync(r => r.Slug == slug);
    }

    public async Task<IEnumerable<Resource>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(r => r.ResourceCategories.Any(rc => rc.CategoryId == categoryId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
