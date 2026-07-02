using DotnetNiger.UI.Helpers;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock;

public class MockPermissionService : IPermissionService
{
    private HashSet<string> _permissions = [];

    public IReadOnlySet<string> Permissions => _permissions;

    public bool HasPermission(string permissionName) => _permissions.Contains(permissionName);

    public async Task LoadPermissionsAsync()
    {
        await Task.Delay(100);
        _permissions =
        [
            PermissionNames.ProfileEdit,
            PermissionNames.CommentCreate,
            PermissionNames.EventRegister,
            PermissionNames.BlogCreate,
            PermissionNames.BlogEdit,
            PermissionNames.BlogDelete,
            PermissionNames.EventCreate,
            PermissionNames.EventEdit,
            PermissionNames.EventDelete,
            PermissionNames.ResourceCreate,
            PermissionNames.ResourceEdit,
            PermissionNames.ResourceDelete,
            PermissionNames.AdminProfileView,
            PermissionNames.AdminMyBlogs,
            PermissionNames.AdminMyEvents,
            PermissionNames.AdminMyResources,
            PermissionNames.AdminBlogCreate,
            PermissionNames.AdminEventCreate,
            PermissionNames.AdminResourceCreate,
        ];
    }

    public void Clear() => _permissions = [];
}
