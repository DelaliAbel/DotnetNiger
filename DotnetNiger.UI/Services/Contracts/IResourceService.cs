using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IResourceService
{
    Task<List<ResourceDto>> GetAllResourcesAsync();
    Task<ResourceDto?> GetResourceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResourceDto?> GetResourceBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<ResourceDto>> GetResourcesByTypeAsync(string resourceType);
    Task<List<ResourceDto>> GetResourcesByLevelAsync(string level);
    Task<List<ResourceDto>> SearchResourcesAsync(string query);
    Task<List<string>> GetResourceTypesAsync();
    Task<List<string>> GetLevelsAsync();
    Task<ResourceDto?> CreateResourceAsync(CreateResourceRequest request, CancellationToken cancellationToken = default);
    Task<ResourceDto?> AddResourceAsync(CreateResourceRequest request, CancellationToken cancellationToken = default);
    Task<ResourceDto?> UpdateResourceAsync(Guid id, CreateResourceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteResourceAsync(Guid id, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ResourceDto>> GetMyResourcesAsync();
}
