using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IEventRegistrationService
{
    Task<EventRegistrationResponse?> RegisterAsync(Guid eventId, Guid userId, string userName, string? avatarUrl);
    Task<bool> CancelRegistrationAsync(Guid eventId, Guid userId);
}
