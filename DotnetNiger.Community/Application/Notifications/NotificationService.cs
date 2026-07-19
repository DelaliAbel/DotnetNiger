using DotnetNiger.Common.Email;
using DotnetNiger.Community.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Community.Application.Notifications;

/// <summary>Notification par email aux abonnés de la newsletter.</summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<NotificationService> _logger;
    private readonly bool _enabled;

    /// <summary>Initialise le service avec la base, le service email, le logger et la config.</summary>
    public NotificationService(AppDbContext db, IEmailService email, ILogger<NotificationService> logger, IConfiguration configuration)
    {
        _db = db;
        _email = email;
        _logger = logger;
        _enabled = configuration.GetValue<bool>("Notifications:EmailEnabled");
    }

    /// <summary>Notifie tous les abonnés actifs de la création d'un nouvel événement.</summary>
    public async Task NotifyNewEventAsync(string title, string description, DateTime startDate)
    {
        var subscribers = await _db.NewsletterSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.Email)
            .ToListAsync();

        if (subscribers.Count == 0) return;

        var body = $"<p>Un nouvel événement a été créé : <strong>{title}</strong></p>" +
                   $"<p>{description}</p>" +
                   $"<p>Date : {startDate:dd/MM/yyyy HH:mm}</p>";

        await SendAsync(subscribers, $"Nouvel événement : {title}", body);
    }

    /// <summary>Notifie les abonnés actifs de la création d'un nouveau projet.</summary>
    public async Task NotifyNewProjectAsync(string title, string description, string authorName)
    {
        var subscribers = await _db.NewsletterSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.Email)
            .ToListAsync();

        if (subscribers.Count == 0) return;

        var body = $"<p>Un nouveau projet a été créé : <strong>{title}</strong></p>" +
                   $"<p>{description}</p>" +
                   $"<p>Auteur : {authorName}</p>";

        await SendAsync(subscribers, $"Nouveau projet : {title}", body);
    }

    /// <summary>Notifie les abonnés d'un nouvel article de blog.</summary>
    public async Task NotifyNewPostAsync(string title, string authorName)
    {
        var emails = await _db.NewsletterSubscriptions
            .Where(s => s.IsActive)
            .Select(s => s.Email)
            .ToListAsync();

        if (emails.Count == 0) return;

        var body = $"<p>Un nouvel article a été publié : <strong>{title}</strong></p>" +
                   $"<p>Auteur : {authorName}</p>";

        await SendAsync(emails, $"Nouvel article : {title}", body);
    }

    private async Task SendAsync(List<string> emails, string subject, string body)
    {
        if (_enabled)
        {
            try
            {
                await _email.SendBatchAsync(emails.ToArray(), subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EMAIL] Échec envoi batch pour {Subject}", subject);
            }
        }
        else
        {
            _logger.LogInformation("[NOTIFICATION] Batch à {Count} abonnés : {Subject}", emails.Count, subject);
        }
    }
}
