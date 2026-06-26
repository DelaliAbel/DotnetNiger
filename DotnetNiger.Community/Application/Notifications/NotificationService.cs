using DotnetNiger.Community.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Community.Application.Notifications;

/// <summary>Notification par email aux abonnés de la newsletter (logge les envois pour l'instant).</summary>
/// <summary>Implémentation du service de notification qui envoie des alertes aux abonnés de la newsletter.</summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>Initialise le service de notification avec la base de données et le logger.</summary>
    /// <param name="db">Contexte de base de données.</param>
    /// <param name="logger">Logger.</param>
    public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Notifie tous les abonnés actifs de la création d'un nouvel événement.</summary>
    /// <param name="title">Titre de l'événement.</param>
    /// <param name="description">Description de l'événement.</param>
    /// <param name="startDate">Date de début.</param>
    public async Task NotifyNewEventAsync(string title, string description, DateTime startDate)
    {
        var subscribers = await _db.NewsletterSubscriptions
            .Where(s => s.IsActive)
            .ToListAsync();

        _logger.LogInformation(
            "[NOTIFICATION] Nouvel événement créé: {Title} ({Count} abonnés notifiés)",
            title, subscribers.Count);

        foreach (var sub in subscribers)
        {
            _logger.LogInformation(
                "[NOTIFICATION] Email à {Email}: Nouvel événement - {Title}",
                sub.Email, title);
        }
    }

    /// <summary>Notifie les abonnés actifs de la création d'un nouveau projet.</summary>
    /// <summary>Notifie tous les abonnés actifs de la création d'un nouveau projet.</summary>
    /// <param name="title">Titre du projet.</param>
    /// <param name="description">Description du projet.</param>
    /// <param name="authorName">Nom de l'auteur.</param>
    public async Task NotifyNewProjectAsync(string title, string description, string authorName)
    {
        var subscribers = await _db.NewsletterSubscriptions
            .Where(s => s.IsActive)
            .ToListAsync();

        _logger.LogInformation(
            "[NOTIFICATION] Nouveau projet créé: {Title} par {Author} ({Count} abonnés notifiés)",
            title, authorName, subscribers.Count);

        foreach (var sub in subscribers)
        {
            _logger.LogInformation(
                "[NOTIFICATION] Email à {Email}: Nouveau projet - {Title}",
                sub.Email, title);
        }
    }
}
