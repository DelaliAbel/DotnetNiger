using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Modération des articles (publication, dépublication).</summary>
public interface IPostModerationService
{
    /// <summary>Publie un article (vérifie les droits).</summary>
    Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Dépublie un article (vérifie les droits).</summary>
    Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin);
}
