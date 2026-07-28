using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de consultation des événements.</summary>
public interface IEventQueryService
{
    /// <summary>Récupère les événements paginés avec filtres.</summary>
    Task<PaginatedResponse<EventResponse>> GetAllAsync(
        string? status, string? query, string? location,
        string? category, string? tag, DateTime? from, DateTime? to,
        Guid? organizerId, int page, int pageSize, Guid? createdBy = null);
    /// <summary>Récupère un événement par identifiant.</summary>
    Task<EventResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère un événement par son slug.</summary>
    Task<EventResponse?> GetBySlugAsync(string slug);
    /// <summary>Récupère les événements en attente de modération.</summary>
    Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page, int pageSize);
    /// <summary>Récupère les inscriptions d'un événement.</summary>
    Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId);
}
