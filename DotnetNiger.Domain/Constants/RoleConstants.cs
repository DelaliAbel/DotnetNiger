namespace DotnetNiger.Domain.Constants;

public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Collaborator = "Collaborator";
    public const string Client = "Client";
    public const string AdminOrSuperAdmin = "SuperAdmin,Admin";

    public static readonly string[] All = [SuperAdmin, Admin, User, Collaborator, Client, AdminOrSuperAdmin];

    public static bool IsValid(string roleName)
        => Array.Exists(All, r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
}
