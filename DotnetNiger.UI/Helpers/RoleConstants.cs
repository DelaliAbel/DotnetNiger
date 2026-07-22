namespace DotnetNiger.UI.Helpers;

public static class RoleConstants
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Collaborator = "Collaborator";
    public const string User = "User";

    public static readonly string[] AdminRoles = [Admin, SuperAdmin];

    public static bool IsAdminRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool IsCollaboratorRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && role.Equals(Collaborator, StringComparison.OrdinalIgnoreCase);
}
