using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Requêtes de consultation des événements.</summary>
public interface IEventQueryService
{
    /// <summary>Recherche paginée avec filtres.</summary>
    Task<PaginatedResponse<EventResponse>> GetAllAsync(string? published, string? past, string? eventType, string? query, string? tag, DateTime? startDateFrom, DateTime? startDateTo, Guid? submitterId = null, int page = 1, int pageSize = 10, Guid? after = null);
    /// <summary>Événements publiés à venir.</summary>
    Task<List<EventResponse>> GetUpcomingAsync(int page = 1, int pageSize = 10);
    /// <summary>Détail par identifiant.</summary>
    Task<EventResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail par slug.</summary>
    Task<EventResponse?> GetBySlugAsync(string slug);
    /// <summary>Événements en attente de validation.</summary>
    Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page = 1, int pageSize = 10);
    /// <summary>Liste des inscriptions à un événement.</summary>
    Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId);
}
