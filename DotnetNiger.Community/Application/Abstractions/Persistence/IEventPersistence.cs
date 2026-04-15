using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IEventPersistence : ICrudPersistence<Event>
{
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10);
}
