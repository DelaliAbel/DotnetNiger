using DotnetNiger.UI.Helpers;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Mock;

public class MockPermissionService : IPermissionService
{
    private HashSet<string> _permissions = [];

    private readonly IUserStateService _userStateService;

    public MockPermissionService(IUserStateService userStateService)
    {
        _userStateService = userStateService;
    }

    public IReadOnlySet<string> Permissions => _permissions;

    public bool HasPermission(string permissionName) => _permissions.Contains(permissionName);

    public async Task LoadPermissionsAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(100);

        var user = _userStateService.CurrentUser;
        if (user is null)
        {
            _permissions = [];
            return;
        }

        var isAdmin = user.Roles.Any(r => RoleConstants.IsAdminRole(r));
        var isSuperAdmin = user.Roles.Any(r => r == RoleConstants.SuperAdmin);
        var isCollaborator = user.Roles.Any(r => RoleConstants.IsCollaboratorRole(r));
        var isCertified = user.IsCertificateValidated;

        var perms = new HashSet<string>();

        if (isSuperAdmin)
        {
            perms.UnionWith(GetAdminPermissions());
            perms.UnionWith(GetSuperAdminPermissions());
        }
        else if (isAdmin)
        {
            perms.UnionWith(GetAdminPermissions());
        }
        else if (isCollaborator && isCertified)
        {
            perms.UnionWith(GetCollaboratorN2Permissions());
        }
        else if (isCollaborator)
        {
            perms.UnionWith(GetCollaboratorN1Permissions());
        }

        _permissions = perms;
    }

    public void Clear() => _permissions = [];

    private static HashSet<string> GetCollaboratorN1Permissions()
    {
        return
        [
            PermissionNames.ProfileEdit,
            PermissionNames.EventRegister,
            PermissionNames.CommentCreate,
        ];
    }

    private static HashSet<string> GetCollaboratorN2Permissions()
    {
        return
        [
            PermissionNames.ProfileEdit,
            PermissionNames.EventRegister,
            PermissionNames.CommentCreate,

            PermissionNames.BlogCreate,
            PermissionNames.BlogEdit,
            PermissionNames.BlogDelete,

            PermissionNames.EventCreate,
            PermissionNames.EventEdit,
            PermissionNames.EventDelete,

            PermissionNames.ResourceCreate,
            PermissionNames.ResourceEdit,
            PermissionNames.ResourceDelete,

            PermissionNames.ProjectCreate,
            PermissionNames.ProjectEdit,
            PermissionNames.ProjectDelete,

            PermissionNames.AdminProfileView,
            PermissionNames.AdminMyBlogs,
            PermissionNames.AdminMyEvents,
            PermissionNames.AdminMyResources,
            PermissionNames.AdminMyProjects,
            PermissionNames.AdminBlogCreate,
            PermissionNames.AdminEventCreate,
            PermissionNames.AdminResourceCreate,
        ];
    }

    private static HashSet<string> GetAdminPermissions()
    {
        return
        [
            PermissionNames.ProfileEdit,
            PermissionNames.EventRegister,
            PermissionNames.CommentCreate,

            PermissionNames.BlogCreate,
            PermissionNames.BlogEdit,
            PermissionNames.BlogDelete,
            PermissionNames.BlogPublish,

            PermissionNames.EventCreate,
            PermissionNames.EventEdit,
            PermissionNames.EventDelete,
            PermissionNames.EventPublish,
            PermissionNames.EventApprove,

            PermissionNames.ResourceCreate,
            PermissionNames.ResourceEdit,
            PermissionNames.ResourceDelete,
            PermissionNames.ResourcePublish,

            PermissionNames.ProjectCreate,
            PermissionNames.ProjectEdit,
            PermissionNames.ProjectDelete,
            PermissionNames.ProjectApprove,

            PermissionNames.AdminProfileView,
            PermissionNames.AdminMyBlogs,
            PermissionNames.AdminMyEvents,
            PermissionNames.AdminMyResources,
            PermissionNames.AdminMyProjects,
            PermissionNames.AdminBlogCreate,
            PermissionNames.AdminEventCreate,
            PermissionNames.AdminResourceCreate,

            PermissionNames.AdminUsersView,
            PermissionNames.AdminUsersManage,

            PermissionNames.AdminCertificatesView,
            PermissionNames.AdminCertificatesApprove,

            PermissionNames.AdminSettingsView,
            PermissionNames.AdminSettingsManage,
        ];
    }

    private static HashSet<string> GetSuperAdminPermissions()
    {
        return
        [
            PermissionNames.AdminRolesManage,
            PermissionNames.AdminPermissionsManage,
        ];
    }
}
