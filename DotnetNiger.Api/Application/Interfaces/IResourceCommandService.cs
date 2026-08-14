using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modification des ressources.</summary>
public interface IResourceCommandService
{
    /// <summary>Crée une ressource.</summary>
    Task<ResourceResponse> CreateAsync(CreateResourceRequest request, Guid authorId, bool isAdmin, bool isCollaborator, CancellationToken ct = default);
    /// <summary>Met à jour une ressource.</summary>
    Task<ResourceResponse?> UpdateAsync(Guid id, UpdateResourceRequest request, Guid userId, bool isAdmin, CancellationToken ct = default);
    /// <summary>Supprime une ressource.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default);
    /// <summary>Incrémente le compteur de vues.</summary>
    Task<ResourceResponse?> IncrementViewCountAsync(Guid id, CancellationToken ct = default);
    /// <summary>Soumet une ressource pour modération.</summary>
    Task SubmitForReviewAsync(Guid id, CancellationToken ct = default);
    /// <summary>Publie une ressource.</summary>
    Task PublishAsync(Guid id, CancellationToken ct = default);
}
