namespace DotnetNiger.Identity.Application;

public static class PermissionNames
{
    public const string ProfileEdit = "profile.edit";
    public const string CommentCreate = "comment.create";
    public const string EventRegister = "event.register";

    public const string BlogCreate = "blog.create";
    public const string BlogEdit = "blog.edit";
    public const string BlogDelete = "blog.delete";
    public const string BlogPublish = "blog.publish";

    public const string EventCreate = "event.create";
    public const string EventEdit = "event.edit";
    public const string EventDelete = "event.delete";
    public const string EventPublish = "event.publish";
    public const string EventApprove = "event.approve";

    public const string ResourceCreate = "resource.create";
    public const string ResourceEdit = "resource.edit";
    public const string ResourceDelete = "resource.delete";
    public const string ResourcePublish = "resource.publish";

    public const string AdminUsersView = "admin.users.view";
    public const string AdminUsersManage = "admin.users.manage";
    public const string AdminRolesManage = "admin.roles.manage";
    public const string AdminPermissionsManage = "admin.permissions.manage";
    public const string AdminCertificatesView = "admin.certificates.view";
    public const string AdminCertificatesApprove = "admin.certificates.approve";
    public const string AdminSettingsView = "admin.settings.view";
    public const string AdminSettingsManage = "admin.settings.manage";
    public const string AdminProfileView = "admin.profile.view";
    public const string AdminMyBlogs = "admin.my.blogs";
    public const string AdminMyEvents = "admin.my.events";
    public const string AdminMyResources = "admin.my.resources";
    public const string AdminBlogCreate = "admin.blog.create";
    public const string AdminEventCreate = "admin.event.create";
    public const string AdminResourceCreate = "admin.resource.create";
    public const string AdminMyProjects = "admin.my.projects";

    public const string ProjectCreate = "project.create";
    public const string ProjectEdit = "project.edit";
    public const string ProjectDelete = "project.delete";
    public const string ProjectApprove = "project.approve";

    public static readonly string[] All =
    [
        ProfileEdit, CommentCreate, EventRegister,

        BlogCreate, BlogEdit, BlogDelete, BlogPublish,
        EventCreate, EventEdit, EventDelete, EventPublish, EventApprove,
        ResourceCreate, ResourceEdit, ResourceDelete, ResourcePublish,

        AdminUsersView, AdminUsersManage, AdminRolesManage, AdminPermissionsManage,
        AdminCertificatesView, AdminCertificatesApprove,
        AdminSettingsView, AdminSettingsManage,
        AdminProfileView, AdminMyBlogs, AdminMyEvents, AdminMyResources, AdminMyProjects,
        AdminBlogCreate, AdminEventCreate, AdminResourceCreate,

        ProjectCreate, ProjectEdit, ProjectDelete, ProjectApprove,
    ];

    public static readonly HashSet<string> AllSet = [.. All];

    public static readonly string[] SuperAdminPermissions = All;

    public static readonly string[] AdminPermissions =
    [
        .. All.Where(p => p is not (AdminSettingsView or AdminSettingsManage)),
    ];

    public static readonly string[] CollaboratorPermissions =
    [
        ProfileEdit, CommentCreate, EventRegister,

        BlogCreate, BlogEdit, BlogDelete,
        EventCreate, EventEdit, EventDelete,
        ResourceCreate, ResourceEdit, ResourceDelete,

        AdminProfileView, AdminMyBlogs, AdminMyEvents, AdminMyResources, AdminMyProjects,
        AdminBlogCreate, AdminEventCreate, AdminResourceCreate,

        ProjectCreate, ProjectEdit, ProjectDelete,
    ];

    public static readonly string[] UserPermissions =
    [
        ProfileEdit, CommentCreate, EventRegister,
    ];

    public static string CategoryOf(string permission) => permission.Split('.')[0] switch
    {
        "profile" => "Profil",
        "comment" => "Commentaires",
        "event" => "Événements",
        "blog" => "Blog",
        "resource" => "Ressources",
        "admin" => "Administration",
        _ => "Autre",
    };
}
