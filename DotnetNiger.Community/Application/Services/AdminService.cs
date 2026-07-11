using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Service d'administration : statistiques et gestion centralisée des utilisateurs.</summary>
public class AdminService(AppDbContext db, IIdentityApiClient identity) : IAdminService
{
    /// <summary>Agrège les statistiques globales de la plateforme.</summary>
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
        var pendingCertificatesCount = await db.Certificates.CountAsync(c => c.Status == "Pending");

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
            PartnersCount = partnersCount,
            PendingCertificatesCount = pendingCertificatesCount
        };
    }

    /// <summary>Liste tous les utilisateurs Identity avec leurs données de profil enrichies (compétences, liens sociaux).</summary>
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

    /// <summary>Détail d'un utilisateur avec son profil membre (retourne null si inconnu d'Identity).</summary>
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

    /// <summary>Active ou désactive un compte utilisateur via l'API Identity.</summary>
    public async Task<bool> UpdateUserStatusAsync(Guid id, bool isActive) => await identity.UpdateUserStatusAsync(id, isActive);

    /// <summary>Définit le statut d'équipe et le poste d'un membre (crée le profil si inexistant).</summary>
    public async Task<bool> UpdateUserTeamAsync(Guid id, bool isTeamMember, string position)
    {
        var member = await db.Members.FindAsync(id);
        if (member is null)
        {
            var identityUser = await identity.GetUserAsync(id);
            member = new Member
            {
                Id = id,
                Email = identityUser?.Email ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            db.Members.Add(member);
        }
        member.IsTeamMember = isTeamMember;
        member.Position = position;
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Crée un utilisateur via Identity, puis son profil membre en base.</summary>
    public async Task<UserDto?> CreateUserAsync(CreateAdminUserRequest request)
    {
        var role = request.IsAdmin ? "Admin" : request.IsCollaborator ? "Collaborator" : null;
        var userId = await identity.RegisterUserAsync(request.Email, request.Password, request.FullName, role);
        if (userId is null || !Guid.TryParse(userId, out var parsedId)) return null;

        var member = new Member
        {
            Id = parsedId,
            Email = request.Email,
            FullName = request.FullName,
            IsTeamMember = request.IsTeamMember,
            Position = request.Position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Members.Add(member);

        if (request.HasApprovedCertificate)
        {
            db.Certificates.Add(new Certificate
            {
                Id = Guid.NewGuid(),
                UserId = parsedId,
                CertificateUrl = "",
                CertificateType = "Automatic",
                Status = "Approved",
                SubmissionDate = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                ReviewedNotes = "Certificat approuvé automatiquement à la création du compte"
            });
        }

        await db.SaveChangesAsync();

        return await GetUserAsync(parsedId);
    }

    /// <summary>Supprime d'abord le compte Identity, puis le profil membre.</summary>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var identityDeleted = await identity.DeleteUserAsync(id);
        if (!identityDeleted) return false;

        var member = await db.Members.FindAsync(id);
        if (member is not null)
        {
            db.Members.Remove(member);
            await db.SaveChangesAsync();
        }
        return true;
    }

    /// <summary>Assigne un rôle à un utilisateur via l'API Identity.</summary>
    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName) => await identity.AssignRoleToUserAsync(userId, roleName);

    /// <summary>Retire un rôle spécifique à un utilisateur via Identity.</summary>
    public async Task<bool> RemoveUserRoleAsync(Guid userId, string roleName) => await identity.RemoveUserRoleAsync(userId, roleName);

    /// <summary>Remplace tous les rôles d'un utilisateur par un seul (supprime les anciens, ajoute le nouveau).</summary>
    public async Task<bool> ReplaceUserRolesAsync(Guid userId, string newRole)
    {
        return await identity.ReplaceUserRolesAsync(userId, newRole);
    }
}
