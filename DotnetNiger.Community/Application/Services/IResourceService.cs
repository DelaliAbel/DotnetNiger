using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

public interface IResourceService
{
    Task<PaginatedResponse<ResourceResponse>> GetAllAsync(string? resourceType, string? level, string? query, string? tag, Guid? categoryId, int page = 1, int pageSize = 10, Guid? after = null);
    Task<ResourceResponse?> GetByIdAsync(Guid id);
    Task<ResourceResponse?> GetBySlugAsync(string slug);
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid userId);
    Task<ResourceResponse?> UpdateAsync(Guid id, CreateResourceRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id);
    Task<List<string>> GetResourceTypesAsync();
    Task<List<string>> GetLevelsAsync();
}
