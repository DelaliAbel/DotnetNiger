using DotnetNiger.Client.Models.Requests;
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IProjectService
{
    Task<PaginatedDto<ProjectResponse>> GetAllAsync(string? status, string? query, int page = 1, int pageSize = 10);
    Task<List<ProjectResponse>> GetFeaturedAsync();
    Task<ProjectResponse?> GetByIdAsync(Guid id);
    Task<ProjectResponse?> GetBySlugAsync(string slug);
    Task<ProjectResponse?> CreateAsync(CreateProjectRequest request);
    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<List<ProjectResponse>> GetMyProjectsAsync();
}
