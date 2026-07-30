namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente une notification envoyée à un utilisateur.
/// </summary>
public class Notification
{
    /// <summary>Identifiant unique de la notification.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur destinataire.</summary>
    public Guid UserId { get; set; }
    /// <summary>Titre de la notification.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Contenu de la notification.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Indique si la notification a été lue.</summary>
    public bool IsRead { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
}
