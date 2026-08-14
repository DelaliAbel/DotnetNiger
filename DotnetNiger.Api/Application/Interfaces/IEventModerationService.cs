using System.Threading;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modération des événements.</summary>
public interface IEventModerationService
{
    /// <summary>Publie un événement.</summary>
    Task<EventResponse?> PublishAsync(Guid id, CancellationToken ct = default);
    /// <summary>Retire un événement de publication.</summary>
    Task<EventResponse?> UnpublishAsync(Guid id, CancellationToken ct = default);
    /// <summary>Approuve un événement.</summary>
    Task<EventResponse?> ApproveAsync(Guid id, CancellationToken ct = default);
    /// <summary>Rejette un événement.</summary>
    Task<EventResponse?> RejectAsync(Guid id, string reason, CancellationToken ct = default);
}
