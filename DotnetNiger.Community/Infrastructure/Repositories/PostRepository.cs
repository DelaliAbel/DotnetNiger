using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 10);
    Task<Post?> GetBySlugAsync(string slug);
    Task<IEnumerable<Post>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Post>> GetByTagAsync(Guid tagId);
}

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Post>> GetPublishedPostsAsync(int page = 1, int pageSize = 10)
    {
        return await _dbSet
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Post?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.PostCategories)
            .Include(p => p.PostTags)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<IEnumerable<Post>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(p => p.IsPublished && p.PostCategories.Any(pc => pc.CategoryId == categoryId))
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByTagAsync(Guid tagId)
    {
        return await _dbSet
            .Where(p => p.IsPublished && p.PostTags.Any(pt => pt.TagId == tagId))
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }
}
