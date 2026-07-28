using DotnetNiger.UI.Helpers;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiEventService : ApiServiceBase, IEventService
{
    public ApiEventService(HttpClient http, ILogger<ApiEventService> logger) : base(http, logger) { }

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        return await GetCollectionAsync<EventDto>(ApiEndpoints.Events);
    }

    public async Task<List<EventDto>> GetAdminEventsAsync(string? status = null)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(status))
            query["status"] = status;
        return await GetCollectionAsync<EventDto>($"{ApiEndpoints.Events}/admin", query);
    }

    public async Task<List<EventDto>> GetPublishedEventsAsync()
    {
        return await GetCollectionAsync<EventDto>(ApiEndpoints.Events);
    }

    public async Task<List<EventDto>> GetUpcomingEventsAsync()
    {
        var events = await GetCollectionAsync<EventDto>(ApiEndpoints.Events);
        return events.Where(e => e.StartDate >= DateTime.Now && e.IsPublished)
            .OrderBy(e => e.StartDate).ToList();
    }

    public async Task<List<EventDto>> GetPastEventsAsync()
    {
        var events = await GetCollectionAsync<EventDto>(ApiEndpoints.Events);
        return events.Where(e => e.EndDate < DateTime.Now && e.IsPublished)
            .OrderByDescending(e => e.StartDate).ToList();
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Events}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<EventDto?> GetEventBySlugAsync(string slug)
    {
        var url = $"{ApiEndpoints.Events}/by-slug/{slug}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<List<EventDto>> SearchEventsAsync(string query)
    {
        return await GetCollectionAsync<EventDto>(ApiEndpoints.Events, new Dictionary<string, string?>
        {
            ["query"] = query
        });
    }

    public async Task<List<EventDto>> GetEventsByTypeAsync(string eventType)
    {
        var events = await GetCollectionAsync<EventDto>(ApiEndpoints.Events, new Dictionary<string, string?>
        {
            ["eventType"] = eventType
        });

        return events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<EventDto?> CreateEventAsync(CreateEventRequest request, Guid currentUserId, bool isAdmin)
    {
        var url = ApiEndpoints.Events;
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ApiResponseReader.ReadErrorAsync(response);
            throw new InvalidOperationException(error ?? $"Erreur {(int)response.StatusCode} lors de la création de l'événement.");
        }
        return await ApiResponseReader.ReadAsync<EventDto>(response);
    }

    public async Task<EventDto?> UpdateEventAsync(Guid id, CreateEventRequest request)
    {
        var url = $"{ApiEndpoints.Events}/{id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Events}/{id}";
        try
        {
            var response = await Http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on DELETE {Url}", url);
            return false;
        }
    }

    public async Task<bool> TogglePublishAsync(Guid id)
    {
        var current = await GetEventByIdAsync(id);
        if (current is null)
            return false;

        var url = current.IsPublished
            ? $"{ApiEndpoints.Events}/{id}/unpublish"
            : $"{ApiEndpoints.Events}/{id}/publish";

        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }

    public async Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName)
    {
        var url = $"{ApiEndpoints.Events}/registrations";
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<EventRegistrationDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId)
    {
        var url = $"{ApiEndpoints.Events}/{eventId}/registrations";
        try
        {
            var response = await Http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on DELETE {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on DELETE {Url}", url);
            return false;
        }
    }

    public async Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId)
    {
        var url = $"{ApiEndpoints.Events}/{eventId}/registrations";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<EventRegistrationDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<EventRegistrationDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<EventRegistrationDto>();
        }
    }

    public async Task<List<EventDto>> GetPendingEventsAsync()
    {
        var url = $"{ApiEndpoints.Events}/pending";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<EventDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<EventDto>();
        }
    }

    public async Task<bool> ApproveEventAsync(Guid eventId, string? adminComment = null)
    {
        var url = string.IsNullOrWhiteSpace(adminComment)
            ? $"{ApiEndpoints.Events}/{eventId}/approve"
            : $"{ApiEndpoints.Events}/{eventId}/approve?comment={Uri.EscapeDataString(adminComment)}";

        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }

    public async Task<bool> RejectEventAsync(Guid eventId, string reason)
    {
        var url = $"{ApiEndpoints.Events}/{eventId}/reject?reason={Uri.EscapeDataString(reason)}";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
            return false;
        }
    }

    public async Task<List<EventDto>> GetEventsBySubmitterAsync(Guid userId)
    {
        var url = $"{ApiEndpoints.Events}?submitterId={userId}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<EventDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<EventDto>();
        }
    }

    public async Task<List<EventDto>> GetMyEventsAsync()
    {
        var url = $"{ApiEndpoints.Events}/mine";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<EventDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<EventDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<EventDto>();
        }
    }

}
