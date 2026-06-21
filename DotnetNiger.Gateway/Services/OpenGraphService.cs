using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DotnetNiger.Gateway.Services;

public class OpenGraphService : IOpenGraphService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenGraphService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OGMetadata?> FetchMetadataAsync(string type, string slug)
    {
        var endpoint = type switch
        {
            "blog" => $"/api/v1/posts/by-slug/{slug}",
            "evenements" or "events" => $"/api/v1/events/by-slug/{slug}",
            "ressources" or "resources" => $"/api/v1/resources/by-slug/{slug}",
            _ => null
        };

        if (endpoint is null) return null;

        try
        {
            using var client = _httpClientFactory.CreateClient("Community");
            var response = await client.GetFromJsonAsync<OpenGraphApiResponse>(endpoint);
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    private class OpenGraphApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public OGMetadata? Data { get; set; }
    }
}
