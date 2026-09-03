namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un abonnement à la newsletter.
/// </summary>
public class NewsletterSubscription
{
    /// <summary>Identifiant unique de l'abonnement.</summary>
    public Guid Id { get; set; }
    /// <summary>Adresse email de l'abonné.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Nom de l'abonné.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Token de désabonnement.</summary>
    public string? UnsubscribeToken { get; set; }
    /// <summary>Indique si l'abonnement est confirmé (double opt-in).</summary>
    public bool IsConfirmed { get; set; } = false;
    /// <summary>Token de confirmation d'inscription.</summary>
    public string? ConfirmationToken { get; set; }
    /// <summary>Date d'expiration du token de confirmation.</summary>
    public DateTime? ConfirmationExpiresAt { get; set; }
    /// <summary>Date de confirmation de l'abonnement.</summary>
    public DateTime? ConfirmedAt { get; set; }
    /// <summary>Indique si l'abonnement est actif.</summary>
    public bool IsActive { get; set; }
    /// <summary>Date d'abonnement.</summary>
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de désabonnement.</summary>
    public DateTime? UnsubscribedAt { get; set; }
}
