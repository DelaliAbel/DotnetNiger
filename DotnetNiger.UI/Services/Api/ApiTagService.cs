using System.Net.Http.Json;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.UI.Services.Api;

public class ApiTagService : ApiServiceBase, ITagService
{
    public ApiTagService(HttpClient http, ILogger<ApiTagService> logger) : base(http, logger) { }

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await GetCollectionAsync<TagDto>(ApiEndpoints.Tags);
    }

    public async Task<TagDto?> GetByIdAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Tags}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<TagDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<TagDto?> GetBySlugAsync(string slug)
    {
        var url = $"{ApiEndpoints.Tags}/{slug}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<TagDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<TagDto?> CreateAsync(string name)
    {
        var url = ApiEndpoints.Tags;
        try
        {
            var content = JsonContent.Create(new { name });
            var response = await Http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<TagDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<TagDto?> UpdateAsync(Guid id, string name)
    {
        var url = $"{ApiEndpoints.Tags}/{id}";
        try
        {
            var content = JsonContent.Create(new { name });
            var response = await Http.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<TagDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var url = $"{ApiEndpoints.Tags}/{id}";
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
}
