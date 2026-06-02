using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    Task<CategoryResponse> CreateAsync(string name, string description);
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string description);
    Task<bool> DeleteAsync(Guid id);
}
