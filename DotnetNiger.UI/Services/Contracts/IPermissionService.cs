namespace DotnetNiger.UI.Services.Contracts;

public interface IPermissionService
{
    IReadOnlySet<string> Permissions { get; }
    bool HasPermission(string permissionName);
    Task LoadPermissionsAsync();
    void Clear();
}
