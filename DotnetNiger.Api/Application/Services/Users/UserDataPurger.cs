using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Api.Application.Services.Users;

/// <summary>
/// Purge proprement toutes les données liées à un utilisateur avant suppression du compte.
/// Évite les violations FK (Restrict/NoAction) et la perte silencieuse de contenu orphelin.
/// </summary>
public static class UserDataPurger
{
    /// <summary>Supprime toutes les données utilisateur (contenus, commentaires, inscriptions, certificats, membre, tokens...).</summary>
    public static async Task PurgeAsync(DotnetNigerDbContext db, Guid userId)
    {
        var userPostIds = await db.Posts.Where(p => p.AuthorId == userId).Select(p => p.Id).ToListAsync();
        var userEventIds = await db.Events.Where(e => e.OrganizerId == userId).Select(e => e.Id).ToListAsync();

        // ---- Commentaires (auteur + commentaires sur les contenus de l'utilisateur + toutes les réponses) ----
        var inScope = await db.Comments
            .Where(c => c.UserId == userId
                || (userPostIds.Count > 0 && c.PostId.HasValue && userPostIds.Contains(c.PostId.Value))
                || (userEventIds.Count > 0 && c.EventId.HasValue && userEventIds.Contains(c.EventId.Value)))
            .ToListAsync();

        var scopeIds = inScope.Select(c => c.Id).ToHashSet();
        var frontier = new HashSet<Guid>(scopeIds);
        var toDelete = new List<Comment>(inScope);

        while (frontier.Count > 0)
        {
            var children = await db.Comments
                .Where(c => c.ParentCommentId.HasValue && frontier.Contains(c.ParentCommentId.Value) && !scopeIds.Contains(c.Id))
                .ToListAsync();
            if (children.Count == 0) break;

            foreach (var child in children)
            {
                scopeIds.Add(child.Id);
                frontier.Add(child.Id);
                toDelete.Add(child);
            }
        }

        db.Comments.RemoveRange(toDelete);

        // ---- Événements de l'utilisateur (commentaires + inscriptions déjà traités) ----
        if (userEventIds.Count > 0)
        {
            var eventRegistrations = await db.EventRegistrations.Where(r => userEventIds.Contains(r.EventId)).ToListAsync();
            db.EventRegistrations.RemoveRange(eventRegistrations);
            var events = await db.Events.Where(e => userEventIds.Contains(e.Id)).ToListAsync();
            db.Events.RemoveRange(events);
        }

        // ---- Publications de l'utilisateur ----
        if (userPostIds.Count > 0)
        {
            var posts = await db.Posts.Where(p => userPostIds.Contains(p.Id)).ToListAsync();
            db.Posts.RemoveRange(posts);
        }

        // ---- Ressources de l'utilisateur ----
        var userResourceIds = await db.Resources.Where(r => r.AuthorId == userId).Select(r => r.Id).ToListAsync();
        if (userResourceIds.Count > 0)
        {
            var resources = await db.Resources.Where(r => userResourceIds.Contains(r.Id)).ToListAsync();
            db.Resources.RemoveRange(resources);
        }

        // ---- Inscriptions aux événements (comme participant) ----
        var registrations = await db.EventRegistrations.Where(r => r.UserId == userId).ToListAsync();
        db.EventRegistrations.RemoveRange(registrations);

        // ---- Certificats (avant les membres, FK NoAction sur MemberId) ----
        var certificates = await db.Certificates.Where(c => c.UserId == userId).ToListAsync();
        db.Certificates.RemoveRange(certificates);

        // ---- Profil membre + compétences + liens sociaux ----
        var members = await db.Members.Where(m => m.UserId == userId).ToListAsync();
        foreach (var member in members)
        {
            var skills = await db.MemberSkills.Where(s => s.MemberId == member.Id).ToListAsync();
            db.MemberSkills.RemoveRange(skills);
            var links = await db.SocialLinks.Where(l => l.MemberId == member.Id).ToListAsync();
            db.SocialLinks.RemoveRange(links);
        }
        db.Members.RemoveRange(members);

        // ---- Interventions (speakers) ----
        var speakers = await db.Speakers.Where(s => s.UserId == userId).ToListAsync();
        db.Speakers.RemoveRange(speakers);

        // ---- Notifications / refresh tokens / historique / consentements / demandes ----
        var notifications = await db.Notifications.Where(n => n.UserId == userId).ToListAsync();
        db.Notifications.RemoveRange(notifications);

        var refreshTokens = await db.RefreshTokens.Where(r => r.UserId == userId).ToListAsync();
        db.RefreshTokens.RemoveRange(refreshTokens);

        var loginHistory = await db.LoginHistories.Where(l => l.UserId == userId).ToListAsync();
        db.LoginHistories.RemoveRange(loginHistory);

        var consents = await db.UserConsents.Where(c => c.UserId == userId).ToListAsync();
        db.UserConsents.RemoveRange(consents);

        var deletionRequests = await db.AccountDeletionRequests.Where(d => d.UserId == userId).ToListAsync();
        db.AccountDeletionRequests.RemoveRange(deletionRequests);

        await db.SaveChangesAsync();
    }
}
