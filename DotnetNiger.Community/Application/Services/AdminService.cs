using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class AdminService(AppDbContext db, IIdentityApiClient identity) : IAdminService
{
    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var postsTask = db.Posts.CountAsync();
        var publishedPostsTask = db.Posts.CountAsync(p => p.IsPublished);
        var draftPostsTask = db.Posts.CountAsync(p => !p.IsPublished);
        var eventsTask = db.Events.CountAsync();
        var upcomingEventsTask = db.Events.CountAsync(e => e.IsPublished && e.EndDate >= now);
        var pastEventsTask = db.Events.CountAsync(e => e.EndDate < now);
        var pendingEventsTask = db.Events.CountAsync(e => !e.IsPublished && !e.IsDeleted);
        var resourcesTask = db.Resources.CountAsync();
        var totalViewsTask = db.Resources.SumAsync(r => r.ViewCount);
        var membersTask = db.Members.CountAsync();
        var newsletterTask = db.NewsletterSubscriptions.CountAsync(s => s.IsActive);
        var commentsTask = db.Comments.CountAsync();
        var projectsTask = db.Projects.CountAsync();
        var partnersTask = db.Partners.CountAsync();

        await Task.WhenAll(postsTask, publishedPostsTask, draftPostsTask, eventsTask, upcomingEventsTask, pastEventsTask, pendingEventsTask, resourcesTask, totalViewsTask, membersTask, newsletterTask, commentsTask, projectsTask, partnersTask);

        return new DashboardResponse
        {
            PostsCount = postsTask.Result,
            PublishedPostsCount = publishedPostsTask.Result,
            DraftPostsCount = draftPostsTask.Result,
            EventsCount = eventsTask.Result,
            UpcomingEventsCount = upcomingEventsTask.Result,
            PastEventsCount = pastEventsTask.Result,
            PendingEventsCount = pendingEventsTask.Result,
            ResourcesCount = resourcesTask.Result,
            TotalResourceViews = totalViewsTask.Result,
            MembersCount = membersTask.Result,
            ActiveNewsletterCount = newsletterTask.Result,
            CommentsCount = commentsTask.Result,
            ProjectsCount = projectsTask.Result,
            PartnersCount = partnersTask.Result
        };
    }

    public async Task<List<UserDto>> GetUsersAsync() => await identity.GetUsersAsync();

    public async Task<UserDto?> GetUserAsync(Guid id) => await identity.GetUserAsync(id);

    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive) => await identity.UpdateUserStatusAsync(id, isActive);

    public async Task<List<RoleDto>> GetRolesAsync() => await identity.GetRolesAsync();

    public async Task<RoleDto?> CreateRoleAsync(string name) => await identity.CreateRoleAsync(name);

    public async Task<List<PermissionDto>> GetPermissionsAsync() => await identity.GetPermissionsAsync();

    public async Task<PermissionDto?> CreatePermissionAsync(string name, string description) => await identity.CreatePermissionAsync(name, description);

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId) => await identity.AssignPermissionToRoleAsync(roleId, permissionId);

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName) => await identity.AssignRoleToUserAsync(userId, roleName);
}
