using System.Net.Http.Json;
using DotnetNiger.UI.Configuration;
using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DotnetNiger.UI.Services.Api;

public class ApiProjectService : ApiServiceBase, IProjectService
{
    public ApiProjectService(HttpClient http, ILogger<ApiProjectService> logger) : base(http, logger) { }

    public async Task<PaginatedDto<ProjectResponse>> GetAllAsync(string? status, string? query, int page = 1, int pageSize = 10)
    {
        var q = new Dictionary<string, string?>
        {
            ["page"] = page.ToString(), ["pageSize"] = pageSize.ToString(),
            ["status"] = status, ["query"] = query
        };
        var url = BuildUrl(ApiEndpoints.Projects, q);
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new PaginatedDto<ProjectResponse>();
            }
            return await ApiResponseReader.ReadAsync<PaginatedDto<ProjectResponse>>(response)
                   ?? new PaginatedDto<ProjectResponse>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new PaginatedDto<ProjectResponse>();
        }
    }

    public async Task<List<ProjectResponse>> GetFeaturedAsync()
    {
        var url = $"{ApiEndpoints.Projects}/featured";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return [];
            }
            return await ApiResponseReader.ReadCollectionAsync<ProjectResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return [];
        }
    }

    public async Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Projects}/{id}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ProjectResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<ProjectResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Projects}/slug/{slug}";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ProjectResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return null;
        }
    }

    public async Task<ProjectResponse?> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var url = ApiEndpoints.Projects;
        var response = await Http.PostAsJsonAsync(url, request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ApiResponseReader.ReadErrorAsync(response);
            throw new InvalidOperationException(error ?? $"Erreur {(int)response.StatusCode} lors de la création du projet.");
        }
        return await ApiResponseReader.ReadAsync<ProjectResponse>(response);
    }

    public async Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Projects}/{id}";
        try
        {
            var response = await Http.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on PUT {Url}", (int)response.StatusCode, url);
                return null;
            }
            return await ApiResponseReader.ReadAsync<ProjectResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on PUT {Url}", url);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{ApiEndpoints.Projects}/{id}";
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

    public async Task<List<ProjectResponse>> GetMyProjectsAsync()
    {
        var url = $"{ApiEndpoints.Projects}/mine";
        try
        {
            var response = await Http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Failed {StatusCode} on GET {Url}", (int)response.StatusCode, url);
                return new List<ProjectResponse>();
            }
            return await ApiResponseReader.ReadCollectionAsync<ProjectResponse>(response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error on GET {Url}", url);
            return new List<ProjectResponse>();
        }
    }
}
