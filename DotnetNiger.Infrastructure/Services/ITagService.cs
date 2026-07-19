using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface ITagService
{
    Task<List<TagResponse>> GetAllAsync();
    Task<TagResponse?> GetByIdAsync(Guid id);
    Task<TagResponse?> GetBySlugAsync(string slug);
    Task<TagResponse> CreateAsync(string name);
    Task<TagResponse?> UpdateAsync(Guid id, string name);
    Task<bool> DeleteAsync(Guid id);
}
