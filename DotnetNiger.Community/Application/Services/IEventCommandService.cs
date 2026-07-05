using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Commandes de modification des événements.</summary>
public interface IEventCommandService
{
    /// <summary>Crée un événement et notifie les abonnés.</summary>
    Task<EventResponse> CreateAsync(CreateEventRequest request, Guid userId);
    /// <summary>Modifie un événement (vérifie le propriétaire ou le rôle admin).</summary>
    Task<EventResponse?> UpdateAsync(Guid id, CreateEventRequest request, Guid userId, bool isAdmin);
    /// <summary>Suppression logique d'un événement.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
}
