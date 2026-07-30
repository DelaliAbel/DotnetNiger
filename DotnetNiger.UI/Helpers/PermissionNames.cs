namespace DotnetNiger.UI.Helpers;

/// <summary>
/// Constantes de permissions alignées avec le backend (DotnetNiger.Api.Constants.Permissions).
/// Utilisées par MockPermissionService et les vérifications côté client.
/// </summary>
public static class PermissionNames
{
    public static class Admin
    {
        public const string DashboardView = "admin.dashboard.view";
        public const string UsersRead = "admin.users.read";
        public const string UsersCreate = "admin.users.create";
        public const string UsersUpdate = "admin.users.update";
        public const string UsersDelete = "admin.users.delete";
        public const string UsersInvite = "admin.users.invite";
        public const string RolesManage = "admin.roles.manage";
        public const string PermissionsManage = "admin.permissions.manage";
        public const string SettingsManage = "admin.settings.manage";
        public const string ClientsManage = "admin.clients.manage";
    }

    public static class Content
    {
        public const string EventsApprove = "content.events.approve";
        public const string EventsModerate = "content.events.moderate";
    }

    public static class Community
    {
        public const string CertificatesSubmit = "community.certificates.submit";
        public const string CertificatesApprove = "community.certificates.approve";
        public const string PartnersManage = "community.partners.manage";
        public const string CategoriesManage = "community.categories.manage";
        public const string TagsManage = "community.tags.manage";
    }

    public const string NewsletterManage = "newsletter.manage";

    // Alias pour compatibilité backward avec MockPermissionService
    public const string ProfileEdit = "community.certificates.submit";
    public const string EventRegister = "community.certificates.submit";
    public const string CommentCreate = "community.certificates.submit";

    public const string BlogCreate = "admin.dashboard.view";
    public const string BlogEdit = "admin.dashboard.view";
    public const string BlogDelete = "admin.dashboard.view";
    public const string BlogPublish = "content.events.moderate";

    public const string EventCreate = "admin.dashboard.view";
    public const string EventEdit = "admin.dashboard.view";
    public const string EventDelete = "admin.dashboard.view";
    public const string EventPublish = "content.events.moderate";
    public const string EventApprove = "content.events.approve";

    public const string ResourceCreate = "admin.dashboard.view";
    public const string ResourceEdit = "admin.dashboard.view";
    public const string ResourceDelete = "admin.dashboard.view";
    public const string ResourcePublish = "content.events.moderate";

    public const string ProjectCreate = "admin.dashboard.view";
    public const string ProjectEdit = "admin.dashboard.view";
    public const string ProjectDelete = "admin.dashboard.view";
    public const string ProjectApprove = "content.events.approve";

    public const string AdminUsersView = "admin.users.read";
    public const string AdminUsersManage = "admin.users.create";
    public const string AdminRolesManage = "admin.roles.manage";
    public const string AdminPermissionsManage = "admin.permissions.manage";
    public const string AdminCertificatesView = "community.certificates.approve";
    public const string AdminCertificatesApprove = "community.certificates.approve";
    public const string AdminSettingsView = "admin.settings.manage";
    public const string AdminSettingsManage = "admin.settings.manage";
    public const string AdminProfileView = "admin.dashboard.view";
    public const string AdminMyBlogs = "admin.dashboard.view";
    public const string AdminMyEvents = "admin.dashboard.view";
    public const string AdminMyResources = "admin.dashboard.view";
    public const string AdminMyProjects = "admin.dashboard.view";
    public const string AdminBlogCreate = "admin.dashboard.view";
    public const string AdminEventCreate = "admin.dashboard.view";
    public const string AdminResourceCreate = "admin.dashboard.view";
}
