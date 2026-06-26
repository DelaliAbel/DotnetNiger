using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<CategoryDto?> CreateAsync(string name, string description);
    Task<CategoryDto?> UpdateAsync(Guid id, string name, string description);
    Task<bool> DeleteAsync(Guid id);
}
