using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modification des événements.</summary>
public interface IEventCommandService
{
    /// <summary>Crée un événement.</summary>
    Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, bool isAdmin, bool isCollaborator, CancellationToken ct = default);
    /// <summary>Met à jour un événement.</summary>
    Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin, CancellationToken ct = default);
    /// <summary>Supprime un événement.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default);
    /// <summary>Soumet un événement pour modération.</summary>
    Task SubmitForReviewAsync(Guid id, CancellationToken ct = default);
    /// <summary>Publie un événement.</summary>
    Task PublishAsync(Guid id, CancellationToken ct = default);
    /// <summary>Annule un événement.</summary>
    Task CancelAsync(Guid id, CancellationToken ct = default);
}
