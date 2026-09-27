namespace DotnetNiger.Application.Common;

/// <summary>
/// Noms des rôles ASP.NET Identity utilisés par dotnetNiger.
/// </summary>
/// <remarks>
/// Il n'existe que deux rôles. L'appartenance à l'équipe est un booléen porté par le
/// membre, pas un rôle. Le traitement des signalements est réservé aux administrateurs.
/// </remarks>
public static class RoleNames
{
    /// <summary>
    /// Rôle administrateur : accès au tableau de bord et à toutes les opérations de modération.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Rôle par défaut attribué à toute inscription validée.
    /// </summary>
    public const string Member = "Member";
}
