using System.Threading;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de consultation des ressources.</summary>
public interface IResourceQueryService
{
    /// <summary>Récupère les ressources paginées avec filtres.</summary>
    Task<PaginatedResponse<ResourceResponse>> GetAllAsync(
        string? resourceType, string? level, string? query,
        string? tag, Guid? categoryId, int page, int pageSize, Guid? after = null, Guid? authorId = null, CancellationToken ct = default);
    /// <summary>Récupère une ressource par identifiant.</summary>
    Task<ResourceResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Récupère une ressource par slug.</summary>
    Task<ResourceResponse?> GetBySlugAsync(string slug, CancellationToken ct = default);
    /// <summary>Récupère les types de ressources disponibles.</summary>
    Task<List<string>> GetResourceTypesAsync(CancellationToken ct = default);
    /// <summary>Récupère les niveaux de difficulté disponibles.</summary>
    Task<List<string>> GetLevelsAsync(CancellationToken ct = default);
}
