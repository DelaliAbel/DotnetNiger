using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface IEventRegistrationService
{
    Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string? avatarUrl);
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);
}
