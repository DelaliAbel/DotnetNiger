using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Modération des événements.</summary>
public interface IEventModerationService
{
    /// <summary>Publie un événement.</summary>
    Task<EventResponse?> PublishAsync(Guid id);
    /// <summary>Dépublie un événement.</summary>
    Task<EventResponse?> UnpublishAsync(Guid id);
    /// <summary>Approuve et publie un événement en attente.</summary>
    Task<EventResponse?> ApproveAsync(Guid id);
    /// <summary>Rejette un événement avec un motif.</summary>
    Task<EventResponse?> RejectAsync(Guid id, string reason);
}
