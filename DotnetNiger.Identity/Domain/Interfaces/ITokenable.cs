// Contrat domaine Identity: ITokenable
namespace DotnetNiger.Identity.Domain.Interfaces;

/// <summary>
/// Contrat pour les entites qui representent un token revocable.
/// </summary>
public interface ITokenable
{
    /// <summary>
    /// Identifiant unique.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Valeur du token.
    /// </summary>
    string Token { get; set; }

    /// <summary>
    /// Date d'expiration du token.
    /// </summary>
    DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Date de revocation (null si actif).
    /// </summary>
    DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Indique si le token est encore valide.
    /// </summary>
    bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}
