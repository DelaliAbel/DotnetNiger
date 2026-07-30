using System.Net.Http.Json;
using DotnetNiger.UI.Configuration;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public abstract class ApiServiceBase
{
    protected readonly HttpClient Http;
    protected readonly ILogger Logger;

    protected ApiServiceBase(HttpClient http, ILogger logger)
    {
        Http = http;
        Logger = logger;
    }

    protected async Task<List<T>> GetCollectionAsync<T>(string path, Dictionary<string, string?>? query = null)
    {
        var url = BuildUrl(path, query);
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Requête échouée {StatusCode} sur GET {Url}", (int)response.StatusCode, url);
                return new List<T>();
            }

            return await ApiResponseReader.ReadCollectionAsync<T>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur réseau sur GET {Url}", url);
            return new List<T>();
        }
    }

    protected static string BuildUrl(string path, Dictionary<string, string?>? query = null)
    {
        if (query is null || query.Count == 0)
            return path;

        var queryString = string.Join("&", query
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

        return string.IsNullOrWhiteSpace(queryString) ? path : $"{path}?{queryString}";
    }
}
