using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

public interface IPostModerationService
{
    Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin);
    Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin);
}
