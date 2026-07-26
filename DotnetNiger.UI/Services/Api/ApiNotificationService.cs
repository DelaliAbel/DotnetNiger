using System.Net.Http.Json;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiNotificationService : ApiServiceBase, INotificationService
{
    public ApiNotificationService(HttpClient http, ILogger<ApiNotificationService> logger) : base(http, logger) { }

    public event Action<Guid>? NotificationsChanged;

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId)
    {
        var url = $"{ApiEndpoints.Notifications}/{userId}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<NotificationDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var url = $"{ApiEndpoints.Notifications}/{userId}/unread-count";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return 0;
            }
            var result = await ApiResponseReader.ReadAsync<UnreadCountResponse>(response);
            return result?.Count ?? 0;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return 0;
        }
    }

    public async Task SendNotificationAsync(Guid userId, string message)
    {
        var url = $"{ApiEndpoints.Notifications}/{userId}";
        try
        {
            var response = await Http.PostAsJsonAsync(url, new { message });
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return;
            }
            NotificationsChanged?.Invoke(userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
        }
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var url = $"{ApiEndpoints.Notifications}/{userId}/{notificationId}/read";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return;
            }
            NotificationsChanged?.Invoke(userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var url = $"{ApiEndpoints.Notifications}/{userId}/read-all";
        try
        {
            var response = await Http.PatchAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PATCH {Url}", (int)response.StatusCode, url);
                return;
            }
            NotificationsChanged?.Invoke(userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PATCH {Url}", url);
        }
    }

    private class UnreadCountResponse
    {
        public int Count { get; set; }
    }
}
