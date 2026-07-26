using System.Net.Http.Json;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiNewsletterService : ApiServiceBase, INewsletterService
{
    public ApiNewsletterService(HttpClient http, ILogger<ApiNewsletterService> logger) : base(http, logger) { }

    public async Task<bool> SubscribeAsync(SubscribeRequest request)
    {
        var url = $"{ApiEndpoints.Newsletters}/subscribe";
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request)
    {
        var url = $"{ApiEndpoints.Newsletters}/unsubscribe";
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return false;
        }
    }

    public async Task<bool> DeleteSubscriberAsync(string email)
    {
        var url = $"{ApiEndpoints.Newsletters}/{email}";
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

    public async Task<List<NewsletterSubscriberDto>> GetAllSubscribersAsync()
    {
        var url = ApiEndpoints.Newsletters;
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<NewsletterSubscriberDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }
}
