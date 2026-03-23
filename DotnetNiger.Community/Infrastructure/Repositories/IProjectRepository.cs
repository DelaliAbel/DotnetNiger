using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Infrastructure.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetBySlugAsync(string slug);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
}
