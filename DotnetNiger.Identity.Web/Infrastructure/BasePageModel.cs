using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Infrastructure;

public abstract class BasePageModel : PageModel
{
    protected readonly IHttpClientFactory Http;
    protected readonly IConfiguration Config;
    protected readonly ILogger Logger;

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected BasePageModel(IHttpClientFactory http, IConfiguration config, ILogger logger)
    {
        Http = http;
        Config = config;
        Logger = logger;
    }

    protected BasePageModel(IHttpClientFactory http, IConfiguration config)
    {
        Http = http;
        Config = config;
        Logger = null!;
    }

    public string Message { get; set; } = "";
    public bool IsError { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    protected string GetIdentityUrl()
    {
        return Config["Identity:BaseUrl"]?.TrimEnd('/') ?? "";
    }

    protected async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = Http.CreateClient();
        var token = await HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected void SetMessage(string msg, bool isError = false)
    {
        Message = msg;
        IsError = isError;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "GET {Url} failed", url);
            return default;
        }
    }

    protected async Task<(T? Data, bool Success)> GetWithStatusAsync<T>(string url)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                SetMessage($"Erreur {response.StatusCode} : {error}", true);
                return (default, false);
            }
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<T>(json, JsonOpts);
            return (data, true);
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "GET {Url} failed", url);
            SetMessage("Erreur de connexion au serveur.", true);
            return (default, false);
        }
    }

    protected async Task<(T? Data, bool Success)> PostAsync<T>(string url, object? body = null)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            HttpContent? content = body != null
                ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                : null;
            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                SetMessage($"Erreur {response.StatusCode} : {error}", true);
                return (default, false);
            }
            if (typeof(T) == typeof(object))
            {
                SetMessage("Opération réussie.");
                return (default, true);
            }
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<T>(json, JsonOpts);
            return (data, true);
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "POST {Url} failed", url);
            SetMessage("Erreur de connexion au serveur.", true);
            return (default, false);
        }
    }

    protected async Task<bool> PutAsync(string url, object? body = null)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            HttpContent? content = body != null
                ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                : null;
            var response = await client.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                SetMessage($"Erreur {response.StatusCode} : {error}", true);
                return false;
            }
            SetMessage("Opération réussie.");
            return true;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "PUT {Url} failed", url);
            SetMessage("Erreur de connexion au serveur.", true);
            return false;
        }
    }

    protected async Task<bool> DeleteAsync(string url)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await client.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                SetMessage($"Erreur {response.StatusCode} : {error}", true);
                return false;
            }
            SetMessage("Suppression réussie.");
            return true;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "DELETE {Url} failed", url);
            SetMessage("Erreur de connexion au serveur.", true);
            return false;
        }
    }
}
