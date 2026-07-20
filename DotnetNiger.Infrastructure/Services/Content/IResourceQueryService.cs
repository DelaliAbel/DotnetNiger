using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface IResourceQueryService
{
    Task<PaginatedResponse<ResourceResponse>> GetAllAsync(
        string? resourceType, string? level, string? query,
        string? tag, Guid? categoryId, int page, int pageSize, Guid? after = null, Guid? authorId = null);
    Task<ResourceResponse?> GetByIdAsync(Guid id);
    Task<ResourceResponse?> GetBySlugAsync(string slug);
    Task<List<string>> GetResourceTypesAsync();
    Task<List<string>> GetLevelsAsync();
}
