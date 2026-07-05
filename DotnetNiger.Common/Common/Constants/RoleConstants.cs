using System;

namespace DotnetNiger.Common.Constants;

/// <summary>
/// Noms des rôles utilisés dans l'application.
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Super administrateur — accès à tout, y compris la gestion des tenants.
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// Administrateur — gestion des contenus et des utilisateurs.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Utilisateur standard — utilisé par Identity ET Community.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Collaborateur — utilisé uniquement par Community pour les contributeurs de contenu.
    /// </summary>
    public const string Collaborator = "Collaborator";

    /// <summary>
    /// Chaîne composite pour les filtres autorisant Admin ou SuperAdmin.
    /// </summary>
    public const string AdminOrSuperAdmin = "SuperAdmin,Admin";

    /// <summary>
    /// Retourne la liste complète des rôles.
    /// </summary>
    public static readonly string[] All = [SuperAdmin, Admin, User, Collaborator, AdminOrSuperAdmin];

    /// <summary>
    /// Vérifie si un nom de rôle est valide.
    /// </summary>
    public static bool IsValid(string roleName)
        => Array.Exists(All, r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
}
