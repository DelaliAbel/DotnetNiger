namespace DotnetNiger.Domain.Constants;

public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Collaborator = "Collaborator";
    public const string Client = "Client";
    public const string AdminOrSuperAdmin = "SuperAdmin,Admin";

    public static readonly string[] All = [SuperAdmin, Admin, User, Collaborator];

    public static bool IsValid(string roleName)
        => All.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));

    public static bool IsAdminOrSuperAdmin(string roleName)
        => roleName == SuperAdmin || roleName == Admin;

    public static string[] SplitRoles(string roles)
        => roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
