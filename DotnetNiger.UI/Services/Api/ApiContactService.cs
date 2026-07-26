using System.Net.Http.Json;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiContactService : ApiServiceBase, IContactService
{
    public ApiContactService(HttpClient http, ILogger<ApiContactService> logger) : base(http, logger) { }

    public async Task<bool> SendAsync(ContactRequest request)
    {
        var url = ApiEndpoints.Contact;
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
}
