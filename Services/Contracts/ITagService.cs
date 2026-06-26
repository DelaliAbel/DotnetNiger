using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(Guid id);
    Task<TagDto?> GetBySlugAsync(string slug);
    Task<TagDto?> CreateAsync(string name);
    Task<TagDto?> UpdateAsync(Guid id, string name);
    Task<bool> DeleteAsync(Guid id);
}
