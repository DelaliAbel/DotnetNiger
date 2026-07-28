using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de consultation des articles.</summary>
public interface IPostQueryService
{
    /// <summary>Récupère les articles paginés avec filtres.</summary>
    Task<PaginatedResponse<PostResponse>> GetAllAsync(
        string? published, string? category, string? tag,
        string? query, int page, int pageSize, Guid? after = null, Guid? authorId = null);
    /// <summary>Récupère un article par identifiant.</summary>
    Task<PostResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère un article par slug.</summary>
    Task<PostResponse?> GetBySlugAsync(string slug);
}
