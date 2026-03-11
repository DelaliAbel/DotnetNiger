using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 10);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10);
    Task<Event> CreateEventAsync(Event @event);
    Task<Event> UpdateEventAsync(Event @event);
    Task<bool> DeleteEventAsync(Guid id);
}