using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(string name, string? description);
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description);
    Task<bool> DeleteAsync(Guid id);
}
