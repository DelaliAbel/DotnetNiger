using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Net.Http.Json;

namespace DotnetNiger.UI.Services.Api;

public class ApiResourceService : IResourceService
{
    private readonly HttpClient _http;
    private const string PublicBase = "api/resources";
    private const string AdminBase = "api/community/admin/resources";

    public ApiResourceService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        return await _http.GetFromJsonAsync<List<ResourceDto>>(PublicBase) ?? new List<ResourceDto>();
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<ResourceDto>($"{PublicBase}/{id}");
    }

    public async Task<ResourceDto?> GetResourceBySlugAsync(string slug)
    {
        var resources = await GetAllResourcesAsync();
        return resources.FirstOrDefault(r => r.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ResourceDto>> GetResourcesByTypeAsync(string resourceType)
    {
        var resources = await GetAllResourcesAsync();
        return resources.Where(r => r.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<ResourceDto>> GetResourcesByLevelAsync(string level)
    {
        var resources = await GetAllResourcesAsync();
        return resources.Where(r => r.Level.Equals(level, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
    {
        var resources = await GetAllResourcesAsync();
        return resources.Where(r =>
                r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.ResourceType.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<string>> GetResourceTypesAsync()
    {
        var resources = await GetAllResourcesAsync();
        return resources.Select(r => r.ResourceType).Distinct().OrderBy(t => t).ToList();
    }

    public async Task<List<string>> GetLevelsAsync()
    {
        var resources = await GetAllResourcesAsync();
        return resources.Select(r => r.Level).Distinct().ToList();
    }

    public async Task<ResourceDto> CreateResourceAsync(CreateResourceRequest request)
    {
        var response = await _http.PostAsJsonAsync(AdminBase, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResourceDto>()
               ?? throw new InvalidOperationException("La réponse API est vide pour la création de la ressource.");
    }

    public async Task<ResourceDto> AddResourceAsync(AddResourceRequest request)
    {
        var response = await _http.PostAsJsonAsync(AdminBase, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ResourceDto>()
               ?? throw new InvalidOperationException("La réponse API est vide pour l'ajout de la ressource.");
    }

    public async Task<ResourceDto?> UpdateResourceAsync(Guid id, CreateResourceRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{AdminBase}/{id}", request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ResourceDto>();
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"{AdminBase}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        await _http.PostAsync($"{PublicBase}/{id}/views", null);
    }
}
