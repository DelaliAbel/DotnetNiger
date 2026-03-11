using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 10)
    {
        var allEvents = await _eventRepository.GetAllAsync();
        return allEvents.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _eventRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetUpcomingEventsAsync(limit);
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetPastEventsAsync(limit);
    }

    public async Task<Event> CreateEventAsync(Event @event)
    {
        @event.Id = Guid.NewGuid();
        @event.CreatedAt = DateTime.UtcNow;
        return await _eventRepository.AddAsync(@event);
    }

    public async Task<Event> UpdateEventAsync(Event @event)
    {
        @event.UpdatedAt = DateTime.UtcNow;
        return await _eventRepository.UpdateAsync(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        return await _eventRepository.DeleteAsync(id);
    }
}