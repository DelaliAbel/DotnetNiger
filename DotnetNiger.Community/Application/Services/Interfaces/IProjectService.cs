using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 10);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(Guid id);
}
