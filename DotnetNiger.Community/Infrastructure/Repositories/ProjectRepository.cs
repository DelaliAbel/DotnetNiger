using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Contributors)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _dbSet
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
