using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISlugGenerator _slugGenerator;

    public ProjectService(IProjectRepository projectRepository, ISlugGenerator slugGenerator)
    {
        _projectRepository = projectRepository;
        _slugGenerator = slugGenerator;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 10)
    {
        // Server-side pagination: Uses database Skip/Take via GetPagedAsync
        // This prevents loading entire table into memory
        return await _projectRepository.GetPagedAsync(page, pageSize);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _projectRepository.GetActiveProjectsAsync();
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.Slug = _slugGenerator.Generate(project.Name);
        return await _projectRepository.AddAsync(project);
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        project.Slug = _slugGenerator.Generate(project.Name);
        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        return await _projectRepository.DeleteAsync(id);
    }
}
