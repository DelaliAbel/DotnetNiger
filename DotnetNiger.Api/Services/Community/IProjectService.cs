using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Community;

public interface IProjectService
{
    Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page, int pageSize);
    Task<List<ProjectResponse>> GetFeaturedAsync();
    Task<ProjectResponse?> GetByIdAsync(Guid id);
    Task<ProjectResponse?> GetBySlugAsync(string slug);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName);
    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
}
