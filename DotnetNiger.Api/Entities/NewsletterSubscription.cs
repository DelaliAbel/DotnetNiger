namespace DotnetNiger.Api.Entities;

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
    /// <summary>Indique si l'abonnement est actif.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Date d'abonnement.</summary>
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de désabonnement.</summary>
    public DateTime? UnsubscribedAt { get; set; }
}
