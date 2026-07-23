using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Interface du service de modification des ressources.</summary>
public interface IResourceCommandService
{
    /// <summary>Crée une ressource.</summary>
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid authorId, bool isAdmin, bool isCollaborator);
    /// <summary>Met à jour une ressource.</summary>
    Task<ResourceResponse?> UpdateAsync(Guid id, UpdateResourceRequest request, Guid userId, bool isAdmin);
    /// <summary>Supprime une ressource.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incrémente le compteur de vues.</summary>
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id);
    /// <summary>Soumet une ressource pour modération.</summary>
    Task SubmitForReviewAsync(Guid id);
    /// <summary>Publie une ressource.</summary>
    Task PublishAsync(Guid id);
}
