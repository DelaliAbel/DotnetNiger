namespace DotnetNiger.Api.Constants;

/// <summary>
/// Constantes des rôles utilisateur et utilitaires de validation.
/// </summary>
public static class RoleConstants
{
    /// <summary>Rôle super administrateur.</summary>
    public const string SuperAdmin = "SuperAdmin";
    /// <summary>Rôle administrateur.</summary>
    public const string Admin = "Admin";
    /// <summary>Rôle utilisateur standard.</summary>
    public const string User = "User";
    /// <summary>Rôle collaborateur.</summary>
    public const string Collaborator = "Collaborator";
    /// <summary>Rôle client.</summary>
    public const string Client = "Client";
    /// <summary>Liste des rôles admin séparés par virgule.</summary>
    public const string AdminOrSuperAdmin = "SuperAdmin,Admin";

    /// <summary>Liste de tous les rôles valides.</summary>
    public static readonly string[] All = [SuperAdmin, Admin, User, Collaborator];

    /// <summary>
    /// Vérifie si le nom de rôle est un rôle valide.
    /// </summary>
    public static bool IsValid(string roleName)
        => All.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Vérifie si le rôle est Admin ou SuperAdmin.
    /// </summary>
    public static bool IsAdminOrSuperAdmin(string roleName)
        => roleName == SuperAdmin || roleName == Admin;

    /// <summary>
    /// Découpe une chaîne de rôles séparés par virgule.
    /// </summary>
    public static string[] SplitRoles(string roles)
        => roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
