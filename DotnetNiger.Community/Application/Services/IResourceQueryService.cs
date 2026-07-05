using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Interface pour la lecture des ressources pédagogiques.</summary>
public interface IResourceQueryService
{
    /// <summary>Recherche paginée avec filtres (type, niveau, tag, catégorie, mot-clé).</summary>
    Task<PaginatedResponse<ResourceResponse>> GetAllAsync(string? resourceType, string? level, string? query, string? tag, Guid? categoryId, int page = 1, int pageSize = 10, Guid? after = null);
    /// <summary>Détail d'une ressource avec ses tags.</summary>
    Task<ResourceResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'une ressource par son slug.</summary>
    Task<ResourceResponse?> GetBySlugAsync(string slug);
    /// <summary>Types de ressources disponibles.</summary>
    Task<List<string>> GetResourceTypesAsync();
    /// <summary>Niveaux disponibles.</summary>
    Task<List<string>> GetLevelsAsync();
}
