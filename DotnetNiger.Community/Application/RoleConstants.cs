namespace DotnetNiger.Community.Application;

/// <summary>Constantes des rôles utilisateur pour l'autorisation et la gestion des accès.</summary>
public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Collaborator = "Collaborator";

    public const string AdminOrSuperAdmin = "SuperAdmin,Admin";
}
