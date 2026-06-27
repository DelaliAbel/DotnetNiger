using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des événements : CRUD, inscriptions et modération.</summary>
public interface IEventService
{
    /// <summary>Recherche paginée avec filtres (publication, passé/futur, type, tag, dates, soumetteur).</summary>
    Task<PaginatedResponse<EventResponse>> GetAllAsync(string? published, string? past, string? eventType, string? query, string? tag, DateTime? startDateFrom, DateTime? startDateTo, Guid? submitterId = null, int page = 1, int pageSize = 10, Guid? after = null);
    /// <summary>Événements publiés à venir, triés par date croissante.</summary>
    Task<List<EventResponse>> GetUpcomingAsync(int page = 1, int pageSize = 10);
    /// <summary>Détail d'un événement avec médias, speakers et tags.</summary>
    Task<EventResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'un événement par son slug.</summary>
    Task<EventResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un événement et notifie les abonnés de la newsletter.</summary>
    Task<EventResponse> CreateAsync(CreateEventRequest request, Guid userId);
    /// <summary>Modifie un événement (vérifie le propriétaire ou le rôle admin).</summary>
    Task<EventResponse?> UpdateAsync(Guid id, CreateEventRequest request, Guid userId, bool isAdmin);
    /// <summary>Suppression logique d'un événement.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Publie un événement.</summary>
    Task<EventResponse?> PublishAsync(Guid id);
    /// <summary>Dépublie un événement.</summary>
    Task<EventResponse?> UnpublishAsync(Guid id);
    /// <summary>Inscrit un participant (vérifie la capacité restante).</summary>
    Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string avatarUrl = "");
    /// <summary>Annule l'inscription d'un participant.</summary>
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);
    /// <summary>Liste des inscriptions à un événement.</summary>
    Task<List<EventRegistrationResponse>> GetRegistrationsAsync(Guid eventId);

    /// <summary>Événements en attente de validation par un admin.</summary>
    Task<PaginatedResponse<EventResponse>> GetPendingEventsAsync(int page = 1, int pageSize = 10);
    /// <summary>Approuve et publie un événement en attente.</summary>
    Task<EventResponse?> ApproveAsync(Guid id);
    /// <summary>Rejette un événement avec un motif.</summary>
    Task<EventResponse?> RejectAsync(Guid id, string reason);
}
