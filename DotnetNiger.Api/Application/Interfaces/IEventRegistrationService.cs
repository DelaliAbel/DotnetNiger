using System.Threading;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service d'inscription aux événements.</summary>
public interface IEventRegistrationService
{
    /// <summary>Inscrit un utilisateur à un événement.</summary>
    Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string? avatarUrl, CancellationToken ct = default);
    /// <summary>Annule l'inscription à un événement.</summary>
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId, CancellationToken ct = default);
}
