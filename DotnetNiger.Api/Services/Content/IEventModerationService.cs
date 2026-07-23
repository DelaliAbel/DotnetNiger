using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Interface du service de modération des événements.</summary>
public interface IEventModerationService
{
    /// <summary>Publie un événement.</summary>
    Task<EventResponse?> PublishAsync(Guid id);
    /// <summary>Retire un événement de publication.</summary>
    Task<EventResponse?> UnpublishAsync(Guid id);
    /// <summary>Approuve un événement.</summary>
    Task<EventResponse?> ApproveAsync(Guid id);
    /// <summary>Rejette un événement.</summary>
    Task<EventResponse?> RejectAsync(Guid id, string reason);
}
