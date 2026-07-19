namespace DotnetNiger.Community.Application.Notifications;

/// <summary>Notification aux abonnés de la newsletter pour les nouveaux contenus.</summary>
public interface INotificationService
{
    /// <summary>Notifie les abonnés d'un nouvel événement.</summary>
    Task NotifyNewEventAsync(string title, string description, DateTime startDate);

    /// <summary>Notifie les abonnés d'un nouveau projet.</summary>
    Task NotifyNewProjectAsync(string title, string description, string authorName);

    /// <summary>Notifie les abonnés d'un nouvel article de blog.</summary>
    Task NotifyNewPostAsync(string title, string authorName);
}
