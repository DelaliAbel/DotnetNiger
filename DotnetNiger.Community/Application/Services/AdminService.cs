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
        var eventsTask = db.Events.CountAsync();
        var upcomingEventsTask = db.Events.CountAsync(e => e.IsPublished && e.EndDate >= now);
        var resourcesTask = db.Resources.CountAsync();
        var membersTask = db.Members.CountAsync();
        var commentsTask = db.Comments.CountAsync();

        await Task.WhenAll(postsTask, publishedPostsTask, eventsTask, upcomingEventsTask, resourcesTask, membersTask, commentsTask);

        return new DashboardResponse
        {
            PostsCount = postsTask.Result,
            PublishedPostsCount = publishedPostsTask.Result,
            EventsCount = eventsTask.Result,
            UpcomingEventsCount = upcomingEventsTask.Result,
            ResourcesCount = resourcesTask.Result,
            MembersCount = membersTask.Result,
            CommentsCount = commentsTask.Result
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
