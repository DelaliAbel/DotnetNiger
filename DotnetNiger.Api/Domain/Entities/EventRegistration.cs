namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente l'inscription d'un utilisateur à un événement.
/// </summary>
public class EventRegistration
{
    /// <summary>Identifiant unique de l'inscription.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'événement.</summary>
    public Guid EventId { get; set; }
    /// <summary>Identifiant de l'utilisateur inscrit.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de l'utilisateur inscrit.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>URL de l'avatar de l'utilisateur.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Date d'inscription.</summary>
    public DateTime RegisteredAt { get; set; }
    /// <summary>Indique si l'utilisateur a assisté à l'événement.</summary>
    public bool IsAttended { get; set; }
    /// <summary>Statut de l'inscription (confirmée, en attente, annulée).</summary>
    public string RegistrationStatus { get; set; } = string.Empty;

    /// <summary>Navigation vers l'événement.</summary>
    public Event Event { get; set; } = null!;
    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
}
