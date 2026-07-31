using System.Threading;
namespace DotnetNiger.UI.Services.Contracts;

public interface IPermissionService
{
    IReadOnlySet<string> Permissions { get; }
    bool HasPermission(string permissionName);
    Task LoadPermissionsAsync(CancellationToken cancellationToken = default);
    void Clear();
}
