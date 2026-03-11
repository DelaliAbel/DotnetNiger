using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<IEnumerable<Member>> GetActiveMembersAsync();
}

public class MemberRepository : BaseRepository<Member>, IMemberRepository
{
    public MemberRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Member>> GetActiveMembersAsync()
    {
        return await _dbSet
            .Include(tm => tm.Skills)
            .Where(tm => tm.IsActive)
            .OrderBy(tm => tm.Name)
            .ToListAsync();
    }
}
