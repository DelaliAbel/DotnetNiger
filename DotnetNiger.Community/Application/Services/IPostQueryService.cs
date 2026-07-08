using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Requêtes de consultation des articles.</summary>
public interface IPostQueryService
{
    /// <summary>Recherche paginée avec filtres.</summary>
    Task<PaginatedResponse<PostResponse>> GetAllAsync(string? published, string? category, string? tag, string? query, int page = 1, int pageSize = 10, Guid? after = null, Guid? authorId = null);
    /// <summary>Détail par identifiant.</summary>
    Task<PostResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail par slug.</summary>
    Task<PostResponse?> GetBySlugAsync(string slug);
}
