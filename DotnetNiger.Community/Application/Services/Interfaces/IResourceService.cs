using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IResourceService
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync(int page = 1, int pageSize = 10);
    Task<Resource?> GetResourceByIdAsync(Guid id);
    Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId);
    Task<Resource> CreateResourceAsync(Resource resource);
    Task<Resource> UpdateResourceAsync(Resource resource);
    Task<bool> DeleteResourceAsync(Guid id);
}