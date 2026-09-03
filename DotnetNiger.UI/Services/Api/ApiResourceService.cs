using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiResourceService : ApiServiceBase, IResourceService
{
    private const string MaxPageSize = "100";

    public ApiResourceService(HttpClient http, ILogger<ApiResourceService> logger) : base(http, logger) { }

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        return await GetCollectionAsync<ResourceDto>(ApiEndpoints.Resources, new Dictionary<string, string?>
        {
            ["pageSize"] = MaxPageSize
        });
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Resources}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<ResourceDto?> GetResourceBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Resources}/{slug}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<List<ResourceDto>> GetResourcesByTypeAsync(string resourceType)
    {
        var resources = await GetCollectionAsync<ResourceDto>(ApiEndpoints.Resources, new Dictionary<string, string?>
        {
            ["resourceType"] = resourceType,
            ["pageSize"] = MaxPageSize
        });

        return resources.Where(r => r.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<ResourceDto>> GetResourcesByLevelAsync(string level)
    {
        var resources = await GetCollectionAsync<ResourceDto>(ApiEndpoints.Resources, new Dictionary<string, string?>
        {
            ["level"] = level,
            ["pageSize"] = MaxPageSize
        });

        return resources.Where(r => r.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
    {
        return await GetCollectionAsync<ResourceDto>(ApiEndpoints.Resources, new Dictionary<string, string?>
        {
            ["query"] = query,
            ["pageSize"] = MaxPageSize
        });
    }

    public async Task<List<string>> GetResourceTypesAsync()
    {
        var url = $"{ApiEndpoints.Resources}/types";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<string>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<List<string>> GetLevelsAsync()
    {
        var url = $"{ApiEndpoints.Resources}/levels";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<string>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<ResourceDto?> CreateResourceAsync(CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.Resources;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<ResourceDto?> AddResourceAsync(CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.Resources;
        try
        {
            var response = await Http.PostAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on POST {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
            return null;
        }
    }

    public async Task<ResourceDto?> UpdateResourceAsync(Guid id, CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Resources}/{id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteResourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Resources}/{id}";
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

    public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Resources}/{id}/views";
        try
        {
            await Http.PostAsync(url, null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on POST {Url}", url);
        }
    }

    public async Task<List<ResourceDto>> GetMyResourcesAsync()
    {
        var url = $"{ApiEndpoints.Resources}/mine";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<ResourceDto>();
            }
            return await ApiResponseReader.ReadCollectionAsync<ResourceDto>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<ResourceDto>();
        }
    }

}
