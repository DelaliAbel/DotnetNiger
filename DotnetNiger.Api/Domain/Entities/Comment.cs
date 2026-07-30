namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un commentaire sur un article ou un événement.
/// </summary>
public class Comment
{
    /// <summary>Identifiant unique du commentaire.</summary>
    public Guid Id { get; set; }
    /// <summary>Contenu du commentaire.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Identifiant de l'utilisateur auteur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Identifiant de l'auteur.</summary>
    public Guid AuthorId { get; set; }
    /// <summary>Nom de l'auteur.</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>Avatar de l'auteur.</summary>
    public string AuthorAvatar { get; set; } = string.Empty;
    /// <summary>Identifiant de l'article associé (si applicable).</summary>
    public Guid? PostId { get; set; }
    /// <summary>Identifiant de l'événement associé (si applicable).</summary>
    public Guid? EventId { get; set; }
    /// <summary>Identifiant du commentaire parent (réponse).</summary>
    public Guid? ParentCommentId { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Navigation vers l'auteur.</summary>
    public ApplicationUser? Author { get; set; }
    /// <summary>Navigation vers l'article.</summary>
    public Post? Post { get; set; }
    /// <summary>Navigation vers l'événement.</summary>
    public Event? Event { get; set; }
    /// <summary>Navigation vers le commentaire parent.</summary>
    public Comment? ParentComment { get; set; }
    /// <summary>Réponses au commentaire.</summary>
    public ICollection<Comment> Replies { get; set; } = [];
}
