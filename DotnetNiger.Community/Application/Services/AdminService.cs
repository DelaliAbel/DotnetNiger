using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Application.Services.Interfaces;


namespace DotnetNiger.Community.Application.Services;

public class AdminService : IAdminService
{
    private readonly IPostRepository _postRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPartnerRepository _partnerRepository;

    public AdminService(
        IPostRepository postRepository,
        IEventRepository eventRepository,
        IProjectRepository projectRepository,
        IResourceRepository resourceRepository,
        ICommentRepository commentRepository,
        IPartnerRepository partnerRepository)
    {
        _postRepository = postRepository;
        _eventRepository = eventRepository;
        _projectRepository = projectRepository;
        _resourceRepository = resourceRepository;
        _commentRepository = commentRepository;
        _partnerRepository = partnerRepository;
    }

    // --- Dashboard ---

    public async Task<object> GetDashboardAsync()
    {
        var totalPosts = await _postRepository.CountAsync();
        var publishedPosts = await _postRepository.CountAsync(p => p.IsPublished);
        var totalEvents = await _eventRepository.CountAsync();
        var publishedEvents = await _eventRepository.CountAsync(e => e.IsPublished);
        var totalProjects = await _projectRepository.CountAsync();
        var activeProjects = await _projectRepository.CountAsync();
        var totalResources = await _resourceRepository.CountAsync();
        var pendingResources = await _resourceRepository.CountAsync(r => !r.IsApproved);
        var approvedResources = await _resourceRepository.CountAsync(r => r.IsApproved);
        var totalComments = await _commentRepository.CountAsync();
        var totalPartners = await _partnerRepository.CountAsync();

        return new
        {
            totals = new
            {
                posts = totalPosts,
                events = totalEvents,
                projects = totalProjects,
                resources = totalResources,
                comments = totalComments,
                partners = totalPartners
            },
            moderation = new
            {
                pendingResources,
                approvedResources
            },
            publication = new
            {
                publishedPosts,
                unpublishedPosts = totalPosts - publishedPosts,
                publishedEvents,
                unpublishedEvents = totalEvents - publishedEvents,
                activeProjects
            },
            generatedAt = DateTime.UtcNow
        };
    }

    // --- Resources ---

    public async Task<IEnumerable<Resource>> GetPendingResourcesAsync(int page = 1, int pageSize = 20)
    {
        var pendingResources = await _resourceRepository.FindAsync(r => !r.IsApproved);
        return pendingResources
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<bool> ModerateResourceAsync(Guid resourceId, bool isApproved)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId);
        if (resource == null) return false;

        if (isApproved)
        {
            resource.IsApproved = true;
            resource.ApprovedAt = DateTime.UtcNow;
            await _resourceRepository.UpdateAsync(resource);
            return true;
        }

        return await _resourceRepository.DeleteAsync(resourceId);
    }

    // --- Posts ---

    public async Task<IEnumerable<Post>> GetAllPostsAsync(int page = 1, int pageSize = 20)
    {
        var all = await _postRepository.GetAllAsync();
        return all
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<bool> PublishPostAsync(Guid postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null) return false;

        post.IsPublished = true;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnpublishPostAsync(Guid postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null) return false;

        post.IsPublished = false;
        post.UpdatedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> DeletePostAsync(Guid postId)
    {
        return await _postRepository.DeleteAsync(postId);
    }

    // --- Events ---

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 20)
    {
        var all = await _eventRepository.GetAllAsync();
        return all
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<bool> PublishEventAsync(Guid eventId)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) return false;

        @event.IsPublished = true;
        @event.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(@event);
        return true;
    }

    public async Task<bool> UnpublishEventAsync(Guid eventId)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) return false;

        @event.IsPublished = false;
        @event.UpdatedAt = DateTime.UtcNow;
        await _eventRepository.UpdateAsync(@event);
        return true;
    }

    public async Task<bool> DeleteEventAsync(Guid eventId)
    {
        return await _eventRepository.DeleteAsync(eventId);
    }

    // --- Comments ---

    public async Task<bool> DeleteCommentAsync(Guid commentId)
    {
        return await _commentRepository.DeleteAsync(commentId);
    }

    // --- Projects ---

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 20)
    {
        var all = await _projectRepository.GetAllAsync();
        return all
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<bool> FeatureProjectAsync(Guid projectId, bool isFeatured)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) return false;

        project.IsFeatured = isFeatured;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepository.UpdateAsync(project);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        return await _projectRepository.DeleteAsync(projectId);
    }
}

