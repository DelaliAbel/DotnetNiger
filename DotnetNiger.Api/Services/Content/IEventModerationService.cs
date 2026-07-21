using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface IEventModerationService
{
    Task<EventResponse?> PublishAsync(Guid id);
    Task<EventResponse?> UnpublishAsync(Guid id);
    Task<EventResponse?> ApproveAsync(Guid id);
    Task<EventResponse?> RejectAsync(Guid id, string reason);
}
