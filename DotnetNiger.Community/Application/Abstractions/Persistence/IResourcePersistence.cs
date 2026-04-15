using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IResourcePersistence : ICrudPersistence<Resource>
{
    Task<Resource?> GetBySlugAsync(string slug);
    Task<IEnumerable<Resource>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
}
