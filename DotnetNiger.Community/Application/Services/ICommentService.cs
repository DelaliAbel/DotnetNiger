using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des commentaires (articles et événements) avec arborescence.</summary>
public interface ICommentService
{
    /// <summary>Commentaires d'un article, organisés en arbre (réponses incluses).</summary>
    Task<List<CommentResponse>> GetByPostIdAsync(Guid postId);
    /// <summary>Commentaires d'un événement, organisés en arbre.</summary>
    Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId);
    /// <summary>Détail d'un commentaire par son identifiant.</summary>
    Task<CommentResponse?> GetByIdAsync(Guid id);
    /// <summary>Ajoute un commentaire (réponse possible via ParentCommentId).</summary>
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string authorName, string authorAvatar);
    /// <summary>Modifie le contenu d'un commentaire (vérifie le propriétaire).</summary>
    Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, bool isAdmin = false);
    /// <summary>Supprime un commentaire. Le masque s'il a des réponses, sauf si deleteAllReplies est vrai.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin = false, bool deleteAllReplies = false);
}
