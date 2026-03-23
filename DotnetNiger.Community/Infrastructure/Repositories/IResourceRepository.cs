using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IResourceRepository : IRepository<Resource>
{
    Task<Resource?> GetBySlugAsync(string slug);
    Task<IEnumerable<Resource>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
}
