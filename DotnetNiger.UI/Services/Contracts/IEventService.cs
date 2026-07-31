using DotnetNiger.UI.Models.Requests;
using DotnetNiger.UI.Models.Responses;
using System.Threading;

namespace DotnetNiger.UI.Services.Contracts;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync();
    Task<List<EventDto>> GetPublishedEventsAsync();
    Task<List<EventDto>> GetUpcomingEventsAsync();
    Task<List<EventDto>> GetPastEventsAsync();
    Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDto?> GetEventBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<EventDto>> SearchEventsAsync(string query);
    Task<List<EventDto>> GetEventsByTypeAsync(string eventType);
    Task<EventDto?> UpdateEventAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TogglePublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventRegistrationDto?> RegisterToEventAsync(RegisterEventRequest request, Guid userId, string userName, CancellationToken cancellationToken = default);
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<EventRegistrationDto>> GetRegistrationsByEventAsync(Guid eventId);

    // Création d'événement (soumis ou publié selon rôle)
    Task<EventDto?> CreateEventAsync(CreateEventRequest request, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);

    // Admin : récupérer les événements en attente
    Task<List<EventDto>> GetPendingEventsAsync();

    // Admin : récupérer tous les événements (tous statuts)
    Task<List<EventDto>> GetAdminEventsAsync(string? status = null);

    // Admin : approuver un événement
    Task<bool> ApproveEventAsync(Guid eventId, string? adminComment = null, CancellationToken cancellationToken = default);

    // Admin : rejeter un événement avec motif
    Task<bool> RejectEventAsync(Guid eventId, string reason, CancellationToken cancellationToken = default);

    // Récupérer les événements créés par un utilisateur (ses soumissions)
    Task<List<EventDto>> GetEventsBySubmitterAsync(Guid userId);

    // Récupérer ses propres événements (via JWT)
    Task<List<EventDto>> GetMyEventsAsync();
}
