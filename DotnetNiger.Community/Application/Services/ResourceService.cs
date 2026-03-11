using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceService(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync(int page = 1, int pageSize = 10)
    {
        var allResources = await _resourceRepository.GetAllAsync();
        return allResources.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<Resource?> GetResourceByIdAsync(Guid id)
    {
        return await _resourceRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId)
    {
        return await _resourceRepository.GetByCategoryAsync(categoryId);
    }

    public async Task<Resource> CreateResourceAsync(Resource resource)
    {
        resource.Id = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        return await _resourceRepository.AddAsync(resource);
    }

    public async Task<Resource> UpdateResourceAsync(Resource resource)
    {
        return await _resourceRepository.UpdateAsync(resource);
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        return await _resourceRepository.DeleteAsync(id);
    }
}