using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public class EventRepository : BaseRepository<Event>, IEventRepository, IEventPersistence
{
    public EventRepository(CommunityDbContext context) : base(context)
    {
    }

    public async Task<Event?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(e => e.Medias)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Slug == slug);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10)
    {
        return await _dbSet
            .Where(e => e.StartDate > DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10)
    {
        return await _dbSet
            .Where(e => e.EndDate < DateTime.UtcNow)
            .OrderByDescending(e => e.EndDate)
            .Take(limit)
            .ToListAsync();
    }
}
