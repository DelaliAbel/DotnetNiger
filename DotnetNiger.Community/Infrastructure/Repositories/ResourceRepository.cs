using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

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

    public async Task<IEnumerable<Resource>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(pageSize, 100); // Cap at 100 for safety
        return await _dbSet
            .Where(r => r.ResourceCategories.Any(rc => rc.CategoryId == categoryId))
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
