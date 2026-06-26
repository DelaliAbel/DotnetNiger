using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des projets de la communauté.</summary>
public interface IProjectService
{
    /// <summary>Recherche paginée des projets (filtre par statut ou mot-clé).</summary>
    Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page = 1, int pageSize = 10);
    /// <summary>Projets mis en avant et publiés.</summary>
    Task<List<ProjectResponse>> GetFeaturedAsync();
    /// <summary>Détail d'un projet.</summary>
    Task<ProjectResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'un projet par son slug.</summary>
    Task<ProjectResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un projet et notifie les abonnés de la newsletter.</summary>
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName);
    /// <summary>Modifie un projet (vérifie le propriétaire ou le rôle admin).</summary>
    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin);
    /// <summary>Suppression logique d'un projet.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
}
