using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services.Content;

public interface IPostQueryService
{
    Task<PaginatedResponse<PostResponse>> GetAllAsync(
        string? published, string? category, string? tag,
        string? query, int page, int pageSize, Guid? after = null, Guid? authorId = null);
    Task<PostResponse?> GetByIdAsync(Guid id);
    Task<PostResponse?> GetBySlugAsync(string slug);
}
