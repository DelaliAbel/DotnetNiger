namespace DotnetNiger.Api.Entities;

/// <summary>
/// Stocke les refresh tokens de l'utilisateur.
/// Permet la déconnexion à distance et la révocation des tokens.
/// Chaque token est lié à un utilisateur et a une date d'expiration.
/// </summary>
public class RefreshToken
{
    /// <summary>Identifiant unique du refresh token.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant de l'utilisateur propriétaire.</summary>
    public Guid UserId { get; set; }

    /// <summary>Valeur hashée du refresh token (SHA256).</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Date de création du token.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Date d'expiration du token.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Date de révocation (null = actif).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>User-Agent du client pour identification.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Adresse IP du client.</summary>
    public string? IpAddress { get; set; }

    /// <summary>True si le token a été remplacé par un nouveau (rotation).</summary>
    public bool IsReplaced { get; set; } = false;

    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
}
