using System.Net.Http.Json;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiSettingsService : ApiServiceBase, ISettingsService
{
    public ApiSettingsService(HttpClient http, ILogger<ApiSettingsService> logger) : base(http, logger) { }

    public async Task<List<SiteSettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetCollectionAsync<SiteSettingDto>(ApiEndpoints.AdminSettings);
    }

    public async Task<SiteSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminSettings}/{key}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<SiteSettingDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<SiteSettingDto?> SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminSettings}/{key}";
        try
        {
            var content = JsonContent.Create(new { value });
            var response = await Http.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<SiteSettingDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> SetBatchAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.AdminSettings;
        try
        {
            var content = JsonContent.Create(new { settings });
            var response = await Http.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return false;
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.AdminSettings}/{key}";
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

    public async Task<PublicSettingsResponse?> GetPublicSettingsAsync()
    {
        try
        {
            var response = await Http.GetAsync(ApiEndpoints.PublicSettings);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, ApiEndpoints.PublicSettings);
                return null;
            }
            return await ApiResponseReader.ReadAsync<PublicSettingsResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", ApiEndpoints.PublicSettings);
            return null;
        }
    }
}
