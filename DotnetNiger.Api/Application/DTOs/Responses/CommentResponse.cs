namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un commentaire.</summary>
public class CommentResponse
{
    /// <summary>Identifiant du commentaire.</summary>
    public Guid Id { get; set; }
    /// <summary>Contenu du commentaire.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Identifiant de l'auteur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de l'auteur.</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>URL de l'avatar de l'auteur.</summary>
    public string AuthorAvatar { get; set; } = string.Empty;
    /// <summary>Identifiant de l'article commenté.</summary>
    public Guid PostId { get; set; }
    /// <summary>Identifiant de l'événement commenté.</summary>
    public Guid EventId { get; set; }
    /// <summary>Identifiant du commentaire parent (réponse).</summary>
    public Guid? ParentCommentId { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime? UpdatedAt { get; set; }
    /// <summary>Date du premier signalement (null si jamais signalé). Réservé à l'admin.</summary>
    public DateTime? ReportedAt { get; set; }
    /// <summary>Réponses enfants au commentaire.</summary>
    public List<CommentResponse> Replies { get; set; } = [];
}
