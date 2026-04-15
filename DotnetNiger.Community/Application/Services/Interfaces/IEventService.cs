using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 10);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10);
    Task<Event> CreateEventAsync(Event @event);
    Task<Event> UpdateEventAsync(Event @event);
    Task<bool> DeleteEventAsync(Guid id);

    // Event Registration - Sécurisé avec JWT UserId
    /// <summary>
    /// Enregistrer l'utilisateur actuel (depuis JWT) à un événement
    /// </summary>
    Task<EventRegistrationResponse> RegisterToEventAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Annuler l'enregistrement (avec vérification de propriété)
    /// </summary>
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Récupérer les enregistrements d'un événement
    /// </summary>
    Task<IEnumerable<EventRegistrationResponse>> GetEventRegistrationsAsync(Guid eventId);

    /// <summary>
    /// Récupérer les enregistrements d'un utilisateur
    /// </summary>
    Task<IEnumerable<EventRegistrationResponse>> GetUserRegistrationsAsync(Guid userId);
}
