// Contrat domaine Identity: IUser
namespace DotnetNiger.Identity.Domain.Interfaces;

/// <summary>
/// Contrat pour l'entite utilisateur.
/// </summary>
public interface IUser
{
    /// <summary>
    /// Identifiant unique.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Nom complet.
    /// </summary>
    string FullName { get; set; }

    /// <summary>
    /// Adresse email.
    /// </summary>
    string? Email { get; set; }

    /// <summary>
    /// Compte actif ou non.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Date de creation du compte.
    /// </summary>
    DateTime CreatedAt { get; set; }
}
