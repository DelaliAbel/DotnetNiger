// Enum domaine Identity: RoleType
namespace DotnetNiger.Identity.Domain.Enums;

/// <summary>
/// Types de roles applicatifs.
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Membre standard de la communaute.
    /// </summary>
    Member,

    /// <summary>
    /// Administrateur avec acces complet.
    /// </summary>
    Admin,

    /// <summary>
    /// Super administrateur avec tous les droits sensibles.
    /// </summary>
    SuperAdmin
}
