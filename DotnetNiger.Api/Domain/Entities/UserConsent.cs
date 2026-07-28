namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente le consentement d'un utilisateur (RGPD).
/// </summary>
public class UserConsent
{
    /// <summary>Identifiant unique du consentement.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Type de consentement (cookies, données personnelles, etc.).</summary>
    public string ConsentType { get; set; } = string.Empty;
    /// <summary>Version de la politique de consentement.</summary>
    public string ConsentVersion { get; set; } = string.Empty;
    /// <summary>Indique si le consentement a été accordé.</summary>
    public bool Granted { get; set; }
    /// <summary>Adresse IP au moment du consentement.</summary>
    public string? IpAddress { get; set; }
    /// <summary>User-Agent au moment du consentement.</summary>
    public string? UserAgent { get; set; }
    /// <summary>Date du consentement.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
