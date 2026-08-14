using System.Threading;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de modération des articles.</summary>
public interface IPostModerationService
{
    /// <summary>Publie un article.</summary>
    Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default);
    /// <summary>Retire un article de publication.</summary>
    Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default);
}
