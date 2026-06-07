using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

public class AdminService(AppDbContext db, IIdentityApiClient identity) : IAdminService
{
    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var postsCount = await db.Posts.CountAsync();
        var publishedPostsCount = await db.Posts.CountAsync(p => p.IsPublished);
        var draftPostsCount = await db.Posts.CountAsync(p => !p.IsPublished);
        var eventsCount = await db.Events.CountAsync();
        var upcomingEventsCount = await db.Events.CountAsync(e => e.IsPublished && e.EndDate >= now);
        var pastEventsCount = await db.Events.CountAsync(e => e.EndDate < now);
        var pendingEventsCount = await db.Events.CountAsync(e => !e.IsPublished && !e.IsDeleted);
        var resourcesCount = await db.Resources.CountAsync();
        var totalResourceViews = await db.Resources.SumAsync(r => r.ViewCount);
        var membersCount = await db.Members.CountAsync();
        var activeNewsletterCount = await db.NewsletterSubscriptions.CountAsync(s => s.IsActive);
        var commentsCount = await db.Comments.CountAsync();
        var projectsCount = await db.Projects.CountAsync();
        var partnersCount = await db.Partners.CountAsync();

        return new DashboardResponse
        {
            PostsCount = postsCount,
            PublishedPostsCount = publishedPostsCount,
            DraftPostsCount = draftPostsCount,
            EventsCount = eventsCount,
            UpcomingEventsCount = upcomingEventsCount,
            PastEventsCount = pastEventsCount,
            PendingEventsCount = pendingEventsCount,
            ResourcesCount = resourcesCount,
            TotalResourceViews = totalResourceViews,
            MembersCount = membersCount,
            ActiveNewsletterCount = activeNewsletterCount,
            CommentsCount = commentsCount,
            ProjectsCount = projectsCount,
            PartnersCount = partnersCount
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
