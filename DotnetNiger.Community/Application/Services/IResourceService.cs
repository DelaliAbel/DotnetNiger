using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des ressources pédagogiques.</summary>
public interface IResourceService
{
    /// <summary>Recherche paginée avec filtres (type, niveau, tag, catégorie, mot-clé).</summary>
    Task<PaginatedResponse<ResourceResponse>> GetAllAsync(string? resourceType, string? level, string? query, string? tag, Guid? categoryId, int page = 1, int pageSize = 10, Guid? after = null);
    /// <summary>Détail d'une ressource avec ses tags.</summary>
    Task<ResourceResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'une ressource par son slug.</summary>
    Task<ResourceResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée une ressource liée à ses catégories et tags.</summary>
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid userId);
    /// <summary>Modifie une ressource (vérifie le propriétaire ou le rôle admin).</summary>
    Task<ResourceResponse?> UpdateAsync(Guid id, CreateResourceRequest request, Guid userId, bool isAdmin);
    /// <summary>Suppression logique d'une ressource.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incrémente le compteur de vues de la ressource.</summary>
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id);
    /// <summary>Types de ressources disponibles.</summary>
    Task<List<string>> GetResourceTypesAsync();
    /// <summary>Niveaux disponibles.</summary>
    Task<List<string>> GetLevelsAsync();
}
