using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface ITeamMemberRepository : IRepository<TeamMember>, ITeamMemberPersistence
{
}

public class TeamMemberRepository : BaseRepository<TeamMember>, ITeamMemberRepository
{
    public TeamMemberRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TeamMember>> GetActiveMembersAsync()
    {
        return await _dbSet
            .Include(tm => tm.Skills)
            .Where(tm => tm.IsActive)
            .OrderBy(tm => tm.Name)
            .ToListAsync();
    }
}
