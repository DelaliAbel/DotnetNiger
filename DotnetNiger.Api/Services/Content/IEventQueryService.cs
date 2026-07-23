using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Interface du service de consultation des événements.</summary>
public interface IEventQueryService
{
    /// <summary>Récupère les événements paginés avec filtres.</summary>
    Task<PaginatedResponse<EventResponse>> GetAllAsync(
        string? status, string? query, string? location,
        string? category, string? tag, DateTime? from, DateTime? to,
        Guid? organizerId, int page, int pageSize);
    /// <summary>Récupère un événement par identifiant.</summary>
    Task<EventResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère les événements en attente de modération.</summary>
    Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page, int pageSize);
    /// <summary>Récupère les inscriptions d'un événement.</summary>
    Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId);
}
