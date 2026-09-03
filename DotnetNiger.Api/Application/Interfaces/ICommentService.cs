using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des commentaires.</summary>
public interface ICommentService
{
    /// <summary>Récupère les commentaires d'un article.</summary>
    Task<List<CommentResponse>> GetByPostIdAsync(Guid postId, CancellationToken ct = default);
    /// <summary>Récupère les commentaires d'un événement.</summary>
    Task<List<CommentResponse>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    /// <summary>Récupère un commentaire par identifiant.</summary>
    Task<CommentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Crée un commentaire.</summary>
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, Guid userId, string userName, string? avatar, CancellationToken ct = default);
    /// <summary>Met à jour un commentaire.</summary>
    Task<CommentResponse?> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, CancellationToken ct = default);
    /// <summary>Supprime un commentaire.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, bool deleteAllReplies, CancellationToken ct = default);
    /// <summary>Récupère tous les commentaires.</summary>
    Task<List<CommentResponse>> GetAllAsync(CancellationToken ct = default);
    /// <summary>
    /// Signale un commentaire. Retourne true si le signalement a été créé,
    /// false si le commentaire n'existe pas, et lève une exception si l'utilisateur a déjà signalé.
    /// </summary>
    Task<bool> ReportAsync(Guid id, Guid userId, CancellationToken ct = default);
}
