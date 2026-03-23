using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Constants;

namespace DotnetNiger.Community.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly ISlugGenerator _slugGenerator;

    public ResourceService(IResourceRepository resourceRepository, ISlugGenerator slugGenerator)
    {
        _resourceRepository = resourceRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync(int page = ValidationConstants.DefaultPage, int pageSize = ValidationConstants.DefaultPageSize)
    {
        // Server-side pagination: Query executed in database, not on client
        // Proper database-side Skip/Take prevents loading entire table into memory
        page = Math.Max(1, page);
        pageSize = Math.Min(pageSize, ValidationConstants.MaxPageSize); // Cap at 100 for safety

        return await _resourceRepository.GetPagedAsync(page, pageSize);
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
        if (resource == null)
            throw new ArgumentNullException(nameof(resource), "Resource cannot be null");

        resource.Id = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        resource.Slug = _slugGenerator.Generate(resource.Title);
        return await _resourceRepository.AddAsync(resource);
    }

    public async Task<Resource> UpdateResourceAsync(Resource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource), "Resource cannot be null");

        if (resource.Id == Guid.Empty)
            throw new ArgumentException("Resource ID cannot be empty", nameof(resource));

        resource.Slug = _slugGenerator.Generate(resource.Title);
        return await _resourceRepository.UpdateAsync(resource);
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        return await _resourceRepository.DeleteAsync(id);
    }
}
