using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des inscriptions aux événements.</summary>
public interface IEventRegistrationService
{
    /// <summary>Inscrit un participant (vérifie la capacité restante).</summary>
    Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string avatarUrl = "");
    /// <summary>Annule l'inscription d'un participant.</summary>
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);
}
