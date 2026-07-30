namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'inscription à un événement.</summary>
public class EventRegistrationResponse
{
    /// <summary>Identifiant de l'inscription.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'événement.</summary>
    public Guid EventId { get; set; }
    /// <summary>Titre de l'événement.</summary>
    public string EventTitle { get; set; } = string.Empty;
    /// <summary>Identifiant de l'utilisateur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de l'utilisateur.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>URL de l'avatar de l'utilisateur.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Date d'inscription.</summary>
    public DateTime RegisteredAt { get; set; }
    /// <summary>Indique si le participant a assisté à l'événement.</summary>
    public bool IsAttended { get; set; }
    /// <summary>Statut de l'inscription (confirmée, en attente, etc.).</summary>
    public string RegistrationStatus { get; set; } = string.Empty;
}
