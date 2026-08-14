using Microsoft.AspNetCore.Authorization;

namespace DotnetNiger.Api.Infrastructure.Auth;

/// <summary>
/// Exigence d'autorisation basée sur une ressource (propriété d'un événement).
/// Utilisée avec <see cref="IAuthorizationService"/> : la ressource est l'identifiant
/// (Guid) de l'événement concerné.
/// </summary>
public class EventOwnershipRequirement : IAuthorizationRequirement
{
}
