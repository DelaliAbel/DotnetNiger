using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Helpers;

namespace DotnetNiger.Client.Services.Mock;

public partial class ResourceService
{
    public async Task<ResourceDto?> CreateResourceAsync(CreateResourceRequest request)
    {
        var newResource = new ResourceDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Level = request.Level,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            Tags = []
        };

        _resources.Add(newResource);
        return await Task.FromResult(newResource);
    }

    public async Task<ResourceDto?> AddResourceAsync(CreateResourceRequest request)
    {
        var newResource = new ResourceDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            Url = request.Url,
            ResourceType = request.ResourceType,
            Level = request.Level,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            Tags = []
        };

        _resources.Add(newResource);
        return await Task.FromResult(newResource);
    }

    public async Task<ResourceDto?> UpdateResourceAsync(Guid id, CreateResourceRequest request)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null) return await Task.FromResult<ResourceDto?>(null);

        resource.Title = request.Title;
        resource.Slug = GenerateSlug(request.Title);
        resource.Description = request.Description;
        resource.Url = request.Url;
        resource.ResourceType = request.ResourceType;
        resource.Level = request.Level;

        return await Task.FromResult<ResourceDto?>(resource);
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null) return await Task.FromResult(false);
        _resources.Remove(resource);
        return await Task.FromResult(true);
    }

    private static string GenerateSlug(string title)
        => StringHelper.GenerateSlug(title);
}
