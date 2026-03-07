using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(c => c.PostCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }
}
