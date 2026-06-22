using DotnetNiger.Community.Domain.Entities;
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

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var identityUsers = await identity.GetUsersAsync();
        var memberIds = identityUsers.Select(u => u.Id).ToList();
        var members = await db.Members
            .Include(m => m.Skills)
            .Include(m => m.SocialLinks)
            .Where(m => memberIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        foreach (var user in identityUsers)
        {
            if (members.TryGetValue(user.Id, out var member))
            {
                user.FullName = member.FullName;
                user.PhoneNumber = member.PhoneNumber;
                user.Bio = member.Bio;
                user.AvatarUrl = member.AvatarUrl;
                user.Country = member.Country;
                user.City = member.City;
                user.IsTeamMember = member.IsTeamMember;
                user.Position = member.Position;
                user.Skills = member.Skills.Select(s => s.Name).ToList();
                user.SocialLinks = member.SocialLinks.Select(sl => new SocialLinkResponse
                {
                    Id = sl.Id,
                    Platform = sl.Platform,
                    Url = sl.Url
                }).ToList();
            }
        }

        return identityUsers;
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        var user = await identity.GetUserAsync(id);
        if (user is null) return null;

        var member = await db.Members
            .Include(m => m.Skills)
            .Include(m => m.SocialLinks)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is not null)
        {
            user.FullName = member.FullName;
            user.PhoneNumber = member.PhoneNumber;
            user.Bio = member.Bio;
            user.AvatarUrl = member.AvatarUrl;
            user.Country = member.Country;
            user.City = member.City;
            user.IsTeamMember = member.IsTeamMember;
            user.Position = member.Position;
            user.Skills = member.Skills.Select(s => s.Name).ToList();
            user.SocialLinks = member.SocialLinks.Select(sl => new SocialLinkResponse
            {
                Id = sl.Id,
                Platform = sl.Platform,
                Url = sl.Url
            }).ToList();
        }

        return user;
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive) => await identity.UpdateUserStatusAsync(id, isActive);

    public async Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null)
        {
            member = new Member { Id = id, CreatedAt = DateTime.UtcNow };
            db.Members.Add(member);
        }
        member.IsTeamMember = isTeamMember;
        member.Position = position;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> CreateUserAsync(CreateAdminUserRequest request)
    {
        var userId = await identity.RegisterUserAsync(request.Email, request.Password, request.FullName);
        if (userId is null || !Guid.TryParse(userId, out var parsedId)) return null;

        var member = new Member
        {
            Id = parsedId,
            FullName = request.FullName,
            IsTeamMember = request.IsTeamMember,
            Position = request.Position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Members.Add(member);
        await db.SaveChangesAsync();

        return await GetUserAsync(parsedId);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var member = await db.Members.FindAsync(id);
        if (member is not null)
        {
            db.Members.Remove(member);
            await db.SaveChangesAsync();
        }
        return await identity.DeleteUserAsync(id);
    }

    public async Task<List<RoleDto>> GetRolesAsync() => await identity.GetRolesAsync();

    public async Task<RoleDto?> CreateRoleAsync(string name) => await identity.CreateRoleAsync(name);

    public async Task<List<PermissionDto>> GetPermissionsAsync() => await identity.GetPermissionsAsync();

    public async Task<PermissionDto?> CreatePermissionAsync(string name, string description) => await identity.CreatePermissionAsync(name, description);

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId) => await identity.AssignPermissionToRoleAsync(roleId, permissionId);

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName) => await identity.AssignRoleToUserAsync(userId, roleName);
}
