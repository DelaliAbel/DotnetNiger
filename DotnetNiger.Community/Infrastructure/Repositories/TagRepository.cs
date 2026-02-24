using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
    }
}
