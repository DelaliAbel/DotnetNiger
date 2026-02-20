// Enum domaine Identity: UserStatus
namespace DotnetNiger.Identity.Domain.Enums;

/// <summary>
/// Statuts possibles d'un compte utilisateur.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Compte actif et utilisable.
    /// </summary>
    Active,

    /// <summary>
    /// Compte desactive par un admin.
    /// </summary>
    Inactive,

    /// <summary>
    /// Compte suspendu temporairement.
    /// </summary>
    Suspended,

    /// <summary>
    /// Compte en attente de verification email.
    /// </summary>
    PendingVerification
}
