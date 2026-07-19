using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<CategoryDto?> CreateAsync(string name, string description);
    Task<CategoryDto?> UpdateAsync(Guid id, string name, string description);
    Task<bool> DeleteAsync(Guid id);
}
