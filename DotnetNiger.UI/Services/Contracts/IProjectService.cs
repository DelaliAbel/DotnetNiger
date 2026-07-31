using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IProjectService
{
    Task<PaginatedDto<ProjectResponse>> GetAllAsync(string? status, string? query, int page = 1, int pageSize = 10);
    Task<List<ProjectResponse>> GetFeaturedAsync();
    Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProjectResponse>> GetMyProjectsAsync();
}
