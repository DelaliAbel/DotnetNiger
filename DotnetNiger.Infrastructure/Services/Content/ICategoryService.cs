using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(string name, string? description);
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description);
    Task<bool> DeleteAsync(Guid id);
}
