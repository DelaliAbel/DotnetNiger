using System.Net.Http.Json;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Api;

public class ApiSettingsService : ApiServiceBase, ISettingsService
{
    public ApiSettingsService(HttpClient http) : base(http)
    {
    }

    public async Task<List<SiteSettingDto>> GetAllAsync()
    {
        return await GetCollectionAsync<SiteSettingDto>(ApiEndpoints.AdminSettings);
    }

    public async Task<SiteSettingDto?> GetByKeyAsync(string key)
    {
        var response = await Http.GetAsync($"{ApiEndpoints.AdminSettings}/{key}");
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<SiteSettingDto>(response);
    }

    public async Task<SiteSettingDto?> SetAsync(string key, string value)
    {
        var content = JsonContent.Create(new { value });
        var response = await Http.PutAsync($"{ApiEndpoints.AdminSettings}/{key}", content);
        if (!response.IsSuccessStatusCode) return null;
        return await ApiResponseReader.ReadAsync<SiteSettingDto>(response);
    }

    public async Task<bool> SetBatchAsync(Dictionary<string, string> settings)
    {
        var content = JsonContent.Create(new { settings });
        var response = await Http.PutAsync(ApiEndpoints.AdminSettings, content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var response = await Http.DeleteAsync($"{ApiEndpoints.AdminSettings}/{key}");
        return response.IsSuccessStatusCode;
    }
}
