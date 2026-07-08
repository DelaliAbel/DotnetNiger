using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Interface pour l'écriture des ressources pédagogiques.</summary>
public interface IResourceCommandService
{
    /// <summary>Crée une ressource liée à ses catégories et tags.</summary>
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid userId, bool isAdmin, bool isCollaborator);
    /// <summary>Modifie une ressource (vérifie le propriétaire ou le rôle admin).</summary>
    Task<ResourceResponse?> UpdateAsync(Guid id, CreateResourceRequest request, Guid userId, bool isAdmin);
    /// <summary>Suppression logique d'une ressource.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incrémente le compteur de vues de la ressource.</summary>
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id);
}
