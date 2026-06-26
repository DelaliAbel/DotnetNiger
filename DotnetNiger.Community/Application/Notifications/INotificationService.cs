namespace DotnetNiger.Community.Application.Notifications;

/// <summary>Notification interne aux abonnés de la newsletter (logge et enverra des emails).</summary>
public interface INotificationService
{
    /// <summary>Notifie les abonnés d'un nouvel événement.</summary>
    Task NotifyNewEventAsync(string title, string description, DateTime startDate);
    /// <summary>Notifie les abonnés d'un nouveau projet.</summary>
    Task NotifyNewProjectAsync(string title, string description, string authorName);
}
