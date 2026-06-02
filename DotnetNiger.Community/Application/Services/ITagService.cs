using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

public interface ITagService
{
    Task<List<TagResponse>> GetAllAsync();
    Task<TagResponse?> GetByIdAsync(Guid id);
    Task<TagResponse?> GetBySlugAsync(string slug);
    Task<TagResponse> CreateAsync(string name);
    Task<TagResponse?> UpdateAsync(Guid id, string name);
    Task<bool> DeleteAsync(Guid id);
}
