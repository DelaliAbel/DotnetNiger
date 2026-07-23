namespace DotnetNiger.UI.Helpers;

public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Collaborator = "Collaborator";
    public const string User = "User";

    public static readonly string[] AdminRoles = [Admin, SuperAdmin];
    public static readonly string[] SuperAdminRoles = [SuperAdmin];

    public static bool IsSuperAdminRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && role.Equals(SuperAdmin, StringComparison.OrdinalIgnoreCase);

    public static bool IsAdminRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool IsCollaboratorRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && role.Equals(Collaborator, StringComparison.OrdinalIgnoreCase);
}
