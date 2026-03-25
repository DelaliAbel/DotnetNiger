using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;

namespace DotnetNiger.UI.Services.Api;

public class ApiEventService : IEventService
{
    private readonly HttpClient _http;
    private const string PublicBase = "api/events";
    private const string AdminBase = "api/community/admin/events";

    public ApiEventService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        return await _http.GetFromJsonAsync<List<EventDto>>(PublicBase) ?? new List<EventDto>();
    }

    public async Task<List<EventDto>> GetPublishedEventsAsync()
    {
        var events = await GetAllEventsAsync();
        return events.Where(e => e.IsPublished).ToList();
    }

    public async Task<List<EventDto>> GetUpcomingEventsAsync()
    {
        return await _http.GetFromJsonAsync<List<EventDto>>($"{PublicBase}/upcoming/10") ?? new List<EventDto>();
    }

    public async Task<List<EventDto>> GetPastEventsAsync()
    {
        var events = await GetAllEventsAsync();
        return events.Where(e => e.EndDate < DateTime.Now).OrderByDescending(e => e.StartDate).ToList();
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<EventDto>($"{PublicBase}/{id}");
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug)
    {
        var events = await GetAllEventsAsync();
        return events.FirstOrDefault(e => e.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<EventDto>> SearchEventsAsync(string query)
    {
        var events = await GetAllEventsAsync();
        return events.Where(e =>
                e.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<EventDto>> GetEventsByTypeAsync(string eventType)
    {
        var events = await GetAllEventsAsync();
        return events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request)
    {
        var response = await _http.PostAsJsonAsync(AdminBase, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventDto>()
               ?? throw new InvalidOperationException("La réponse API est vide pour la création de l'événement.");
    }

    public async Task<EventDto?> UpdateEventAsync(Guid id, CreateEventRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{AdminBase}/{id}", request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EventDto>();
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{AdminBase}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TogglePublishAsync(Guid id)
    {
        var current = await GetEventByIdAsync(id);
        if (current is null)
            return false;

        var endpoint = current.IsPublished
            ? $"{AdminBase}/{id}/unpublish"
            : $"{AdminBase}/{id}/publish";

        var response = await _http.PatchAsync(endpoint, null);
        return response.IsSuccessStatusCode;
    }

    public async Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName)
    {
        var payload = new
        {
            request.EventId,
            UserId = userId,
            UserName = userName
        };

        var response = await _http.PostAsJsonAsync("api/events/registrations", payload);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EventRegistrationDto>();
    }

    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var response = await _http.DeleteAsync($"api/events/{eventId}/registrations/{userId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId)
    {
        return await _http.GetFromJsonAsync<List<EventRegistrationDto>>($"api/events/{eventId}/registrations") ?? new List<EventRegistrationDto>();
    }
}
