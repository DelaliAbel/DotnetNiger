using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync();
    Task<List<EventDto>> GetPublishedEventsAsync();
    Task<List<EventDto>> GetUpcomingEventsAsync();
    Task<List<EventDto>> GetPastEventsAsync();
    Task<EventDto?> GetEventByIdAsync(Guid id);
    Task<EventDto?> GetEventBySlugAsync(string slug);
    Task<List<EventDto>> SearchEventsAsync(string query);
    Task<List<EventDto>> GetEventsByTypeAsync(string eventType);
    Task<EventDto> CreateEventAsync(CreateEventRequest request);
    Task<EventDto?> UpdateEventAsync(Guid id, CreateEventRequest request);
    Task<bool> DeleteEventAsync(Guid id);
    Task<bool> TogglePublishAsync(Guid id);
    Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName);
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);
    Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId);
}
