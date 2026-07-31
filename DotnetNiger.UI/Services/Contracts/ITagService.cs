using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TagDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<TagDto?> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<TagDto?> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
