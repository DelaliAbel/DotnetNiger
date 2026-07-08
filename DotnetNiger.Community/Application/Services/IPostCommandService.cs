using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Commandes de modification des articles.</summary>
public interface IPostCommandService
{
    /// <summary>Crée un article.</summary>
    Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName, bool isAdmin, bool isCollaborator);
    /// <summary>Modifie un article (vérifie le propriétaire ou le rôle admin).</summary>
    Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin);
    /// <summary>Supprime définitivement un article.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incémente le compteur de vues.</summary>
    Task<PostResponse?> IncrementViewCountAsync(Guid id);
}
