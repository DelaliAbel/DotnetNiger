using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IAdminService
{
    // Dashboard
    Task<object> GetDashboardAsync();

    // Resources
    Task<IEnumerable<Resource>> GetPendingResourcesAsync(int page = 1, int pageSize = 20);
    Task<bool> ModerateResourceAsync(Guid resourceId, bool isApproved);

    // Posts
    Task<IEnumerable<Post>> GetAllPostsAsync(int page = 1, int pageSize = 20);
    Task<bool> PublishPostAsync(Guid postId);
    Task<bool> UnpublishPostAsync(Guid postId);
    Task<bool> DeletePostAsync(Guid postId);

    // Events
    Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 20);
    Task<bool> PublishEventAsync(Guid eventId);
    Task<bool> UnpublishEventAsync(Guid eventId);
    Task<bool> DeleteEventAsync(Guid eventId);

    // Comments
    Task<bool> DeleteCommentAsync(Guid commentId);

    // Projects
    Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 20);
    Task<bool> FeatureProjectAsync(Guid projectId, bool isFeatured);
    Task<bool> DeleteProjectAsync(Guid projectId);
}
