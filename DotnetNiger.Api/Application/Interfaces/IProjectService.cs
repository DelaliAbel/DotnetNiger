using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des projets.</summary>
public interface IProjectService
{
    /// <summary>Récupère les projets paginés.</summary>
    Task<PaginatedResponse<ProjectResponse>> GetAllAsync(string? status, string? query, int page, int pageSize, Guid? createdBy = null);
    /// <summary>Récupère les projets mis en avant.</summary>
    Task<List<ProjectResponse>> GetFeaturedAsync();
    /// <summary>Récupère un projet par identifiant.</summary>
    Task<ProjectResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère un projet par slug.</summary>
    Task<ProjectResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un projet.</summary>
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, string authorName);
    /// <summary>Met à jour un projet.</summary>
    Task<ProjectResponse?> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, bool isAdmin);
    /// <summary>Supprime un projet.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
}
