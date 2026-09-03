namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Signalement d'un commentaire par un utilisateur.
/// </summary>
public class CommentReport
{
    /// <summary>Identifiant unique du signalement.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant du commentaire signalé.</summary>
    public Guid CommentId { get; set; }
    /// <summary>Identifiant de l'utilisateur ayant signalé.</summary>
    public Guid UserId { get; set; }
    /// <summary>Date du signalement.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Commentaire signalé.</summary>
    public Comment? Comment { get; set; }
    /// <summary>Utilisateur ayant signalé.</summary>
    public ApplicationUser? User { get; set; }
}
