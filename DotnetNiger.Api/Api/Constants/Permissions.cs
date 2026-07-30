namespace DotnetNiger.Api.Constants;

/// <summary>
/// Définition des permissions et rôles de l'application.
/// </summary>
public static class Permissions
{
    /// <summary>Permissions liées à l'administration.</summary>
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

    /// <summary>Permissions liées au contenu.</summary>
    public static class Content
    {
        public const string EventsApprove = "content.events.approve";
        public const string EventsModerate = "content.events.moderate";
    }

    /// <summary>Permissions liées à la communauté.</summary>
    public static class Community
    {
        public const string CertificatesSubmit = "community.certificates.submit";
        public const string CertificatesApprove = "community.certificates.approve";
        public const string PartnersManage = "community.partners.manage";
        public const string CategoriesManage = "community.categories.manage";
        public const string TagsManage = "community.tags.manage";
    }

    /// <summary>Permission de gestion de la newsletter.</summary>
    public const string NewsletterManage = "newsletter.manage";

    /// <summary>Liste de toutes les permissions disponibles.</summary>
    public static string[] All =>
    [
        Admin.DashboardView,
        Admin.UsersRead, Admin.UsersCreate, Admin.UsersUpdate, Admin.UsersDelete, Admin.UsersInvite,
        Admin.RolesManage, Admin.PermissionsManage, Admin.SettingsManage, Admin.ClientsManage,
        Content.EventsApprove, Content.EventsModerate,
        Community.CertificatesSubmit, Community.CertificatesApprove,
        Community.PartnersManage, Community.CategoriesManage, Community.TagsManage,
        NewsletterManage
    ];

    /// <summary>Permissions du SuperAdmin (toutes).</summary>
    public static string[] SuperAdminPermissions => All;

    /// <summary>Permissions de l'Admin standard.</summary>
    public static string[] AdminPermissions =>
    [
        Admin.DashboardView, Admin.UsersRead,
        Content.EventsApprove, Content.EventsModerate,
        Community.CertificatesApprove,
        Community.PartnersManage, Community.CategoriesManage, Community.TagsManage,
        NewsletterManage
    ];

    /// <summary>Permissions du Collaborator (contenu personnel + modération).</summary>
    public static string[] CollaboratorPermissions =>
    [
        Community.CertificatesSubmit,
        Content.EventsModerate
    ];

    /// <summary>Permissions de l'utilisateur standard.</summary>
    public static string[] UserPermissions =>
    [
        Community.CertificatesSubmit
    ];
}
