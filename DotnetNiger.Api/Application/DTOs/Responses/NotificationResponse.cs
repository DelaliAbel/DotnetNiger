namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'une notification utilisateur.</summary>
public class NotificationResponse
{
    /// <summary>Identifiant de la notification.</summary>
    public Guid Id { get; set; }
    /// <summary>Contenu du message de la notification.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Date et heure de création.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Indique si la notification a été lue.</summary>
    public bool IsRead { get; set; }
}
