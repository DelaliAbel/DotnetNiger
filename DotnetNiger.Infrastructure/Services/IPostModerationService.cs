using DotnetNiger.Domain.DTOs.Responses;

namespace DotnetNiger.Infrastructure.Services;

public interface IPostModerationService
{
    Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin);
    Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin);
}
