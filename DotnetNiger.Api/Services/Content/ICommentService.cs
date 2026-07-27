using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Interface du service de gestion des commentaires.</summary>
public interface ICommentService
{
    /// <summary>Récupère les commentaires d'un article.</summary>
    Task<List<CommentResponse>> GetByPostIdAsync(Guid postId);
    /// <summary>Récupère les commentaires d'un événement.</summary>
    Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId);
    /// <summary>Récupère un commentaire par identifiant.</summary>
    Task<CommentResponse?> GetByIdAsync(Guid id);
    /// <summary>Crée un commentaire.</summary>
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string userName, string? avatar);
    /// <summary>Met à jour un commentaire.</summary>
    Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId);
    /// <summary>Supprime un commentaire.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, bool deleteAllReplies);
    /// <summary>Récupère tous les commentaires.</summary>
    Task<List<CommentResponse>> GetAllAsync();
}
