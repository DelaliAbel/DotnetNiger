using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modification des événements.</summary>
public interface IEventCommandService
{
    /// <summary>Crée un événement.</summary>
    Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, bool isAdmin, bool isCollaborator);
    /// <summary>Met à jour un événement.</summary>
    Task<EventResponse?> UpdateAsync(Guid id, UpdateEventRequest request, Guid userId, bool isAdmin);
    /// <summary>Supprime un événement.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Soumet un événement pour modération.</summary>
    Task SubmitForReviewAsync(Guid id);
    /// <summary>Publie un événement.</summary>
    Task PublishAsync(Guid id);
    /// <summary>Annule un événement.</summary>
    Task CancelAsync(Guid id);
}
