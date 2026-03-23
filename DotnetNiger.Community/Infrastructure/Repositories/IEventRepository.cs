using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetBySlugAsync(string slug);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10);
}
