using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class ResourceService : IResourceService
{
    private readonly IAuthService _authService;
    private List<ResourceDto> _resources;

    public ResourceService(IAuthService authService)
    {
        _authService = authService;
        _resources = [];
        InitializeSeedData();
    }

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.OrderByDescending(r => r.CreatedAt).ToList());
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(Guid id)
    {
        await Task.Delay(800);
        return await Task.FromResult(_resources.FirstOrDefault(r => r.Id == id));
    }

    public async Task<ResourceDto?> GetResourceBySlugAsync(string slug)
    {
        await Task.Delay(800);
        var resource = _resources.FirstOrDefault(r => r.Slug == slug);
        return await Task.FromResult(resource);
    }

    public async Task<List<ResourceDto>> GetResourcesByTypeAsync(string resourceType)
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.Where(r => r.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase))
                      .OrderByDescending(r => r.ViewCount)
                      .ToList());
    }

    public async Task<List<ResourceDto>> GetResourcesByLevelAsync(string level)
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.Where(r => r.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
                      .OrderByDescending(r => r.ViewCount)
                      .ToList());
    }

    public async Task<List<ResourceDto>> SearchResourcesAsync(string query)
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.Where(r =>
                    r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.ResourceType.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    r.Tags.Any(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(r => r.ViewCount)
                .ToList());
    }

    public async Task<List<string>> GetResourceTypesAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.Select(r => r.ResourceType).Distinct().OrderBy(t => t).ToList());
    }

    public async Task<List<string>> GetLevelsAsync()
    {
        await Task.Delay(800);
        return await Task.FromResult(
            _resources.Select(r => r.Level).Distinct().OrderBy(l => l switch
            {
                "Débutant" => 0,
                "Intermédiaire" => 1,
                "Avancé" => 2,
                "Tous niveaux" => 3,
                _ => 4
            }).ToList());
    }

    public async Task<List<ResourceDto>> GetMyResourcesAsync()
    {
        await Task.Delay(800);
        var user = await _authService.GetCurrentUserAsync();
        if (user is null) return new();
        return _resources.Where(r => r.CreatedBy == user.Id).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is not null) resource.ViewCount++;
        await Task.CompletedTask;
    }
}
