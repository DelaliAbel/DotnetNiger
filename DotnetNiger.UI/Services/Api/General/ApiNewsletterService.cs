using System.Net.Http.Json;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Api;

public class ApiNewsletterService : ApiServiceBase, INewsletterService
{
    public ApiNewsletterService(HttpClient http) : base(http) { }

    public async Task<bool> SubscribeAsync(SubscribeRequest request)
    {
        var response = await Http.PostAsJsonAsync($"{ApiEndpoints.Newsletters}/subscribe", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request)
    {
        var response = await Http.PostAsJsonAsync($"{ApiEndpoints.Newsletters}/unsubscribe", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSubscriberAsync(string email)
    {
        var response = await Http.DeleteAsync($"{ApiEndpoints.Newsletters}/{email}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<NewsletterSubscriberDto>> GetAllSubscribersAsync()
    {
        var response = await Http.GetAsync(ApiEndpoints.Newsletters);
        if (!response.IsSuccessStatusCode)
            return [];

        return await ApiResponseReader.ReadCollectionAsync<NewsletterSubscriberDto>(response);
    }
}
