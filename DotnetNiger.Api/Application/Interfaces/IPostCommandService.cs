using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modification des articles.</summary>
public interface IPostCommandService
{
    /// <summary>Crée un article.</summary>
    Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName, bool isAdmin, bool isCollaborator);
    /// <summary>Met à jour un article.</summary>
    Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin);
    /// <summary>Supprime un article.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incrémente le compteur de vues.</summary>
    Task<PostResponse?> IncrementViewCountAsync(Guid id);
    /// <summary>Soumet un article pour modération.</summary>
    Task SubmitForReviewAsync(Guid id);
    /// <summary>Publie un article.</summary>
    Task PublishAsync(Guid id);
    /// <summary>Archive un article.</summary>
    Task ArchiveAsync(Guid id);
}
