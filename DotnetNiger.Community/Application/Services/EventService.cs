using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Constants;

namespace DotnetNiger.Community.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ISlugGenerator _slugGenerator;

    public EventService(IEventRepository eventRepository, ISlugGenerator slugGenerator)
    {
        _eventRepository = eventRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int page = ValidationConstants.DefaultPage, int pageSize = ValidationConstants.DefaultPageSize)
    {
        // Server-side pagination: Query executed in database, not on client
        // Proper database-side Skip/Take prevents loading entire table into memory
        page = Math.Max(1, page);
        pageSize = Math.Min(pageSize, ValidationConstants.MaxPageSize); // Cap at 100 for safety

        return await _eventRepository.GetPagedAsync(page, pageSize);
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
        if (@event == null)
            throw new ArgumentNullException(nameof(@event), "Event cannot be null");

        @event.Id = Guid.NewGuid();
        @event.CreatedAt = DateTime.UtcNow;
        @event.Slug = _slugGenerator.Generate(@event.Title);
        return await _eventRepository.AddAsync(@event);
    }

    public async Task<Event> UpdateEventAsync(Event @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event), "Event cannot be null");

        if (@event.Id == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty", nameof(@event));

        @event.UpdatedAt = DateTime.UtcNow;
        @event.Slug = _slugGenerator.Generate(@event.Title);
        return await _eventRepository.UpdateAsync(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        return await _eventRepository.DeleteAsync(id);
    }
}
