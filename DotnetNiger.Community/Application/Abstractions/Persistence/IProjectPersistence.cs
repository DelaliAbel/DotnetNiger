using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Abstractions.Persistence;

public interface IProjectPersistence : ICrudPersistence<Project>
{
    Task<Project?> GetBySlugAsync(string slug);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
}
