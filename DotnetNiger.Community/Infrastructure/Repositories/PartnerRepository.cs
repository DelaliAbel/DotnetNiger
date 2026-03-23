using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public class PartnerRepository : BaseRepository<Partner>, IPartnerRepository
{
    public PartnerRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Partner?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }
}
