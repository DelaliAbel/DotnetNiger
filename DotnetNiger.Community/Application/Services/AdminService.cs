using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Repositories;

namespace DotnetNiger.Community.Application.Services;

public interface IAdminService
{
    Task<object> GetDashboardAsync();
    Task<IEnumerable<Comment>> GetPendingCommentsAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<Resource>> GetPendingResourcesAsync(int page = 1, int pageSize = 20);
    Task<bool> ModerateCommentAsync(Guid commentId, bool isApproved);
    Task<bool> ModerateResourceAsync(Guid resourceId, bool isApproved);
}

public class AdminService : IAdminService
{
    private readonly IPostRepository _postRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IPartnerRepository _partnerRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;

    public AdminService(
        IPostRepository postRepository,
        IEventRepository eventRepository,
        IProjectRepository projectRepository,
        IResourceRepository resourceRepository,
        ICommentRepository commentRepository,
        IPartnerRepository partnerRepository,
        ITeamMemberRepository teamMemberRepository)
    {
        _postRepository = postRepository;
        _eventRepository = eventRepository;
        _projectRepository = projectRepository;
        _resourceRepository = resourceRepository;
        _commentRepository = commentRepository;
        _partnerRepository = partnerRepository;
        _teamMemberRepository = teamMemberRepository;
    }

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
        var pendingComments = await _commentRepository.CountAsync(c => !c.IsApproved);
        var approvedComments = await _commentRepository.CountAsync(c => c.IsApproved);
        var totalPartners = await _partnerRepository.CountAsync();
        var totalTeamMembers = await _teamMemberRepository.CountAsync();

        return new
        {
            totals = new
            {
                posts = totalPosts,
                events = totalEvents,
                projects = totalProjects,
                resources = totalResources,
                comments = totalComments,
                partners = totalPartners,
                teamMembers = totalTeamMembers
            },
            moderation = new
            {
                pendingComments,
                approvedComments,
                pendingResources,
                approvedResources
            },
            publication = new
            {
                publishedPosts,
                publishedEvents,
                activeProjects
            },
            generatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<Comment>> GetPendingCommentsAsync(int page = 1, int pageSize = 20)
    {
        var pendingComments = await _commentRepository.FindAsync(c => !c.IsApproved);
        return pendingComments
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<IEnumerable<Resource>> GetPendingResourcesAsync(int page = 1, int pageSize = 20)
    {
        var pendingResources = await _resourceRepository.FindAsync(r => !r.IsApproved);
        return pendingResources
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<bool> ModerateCommentAsync(Guid commentId, bool isApproved)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            return false;
        }

        if (isApproved)
        {
            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _commentRepository.UpdateAsync(comment);
            return true;
        }

        return await _commentRepository.DeleteAsync(commentId);
    }

    public async Task<bool> ModerateResourceAsync(Guid resourceId, bool isApproved)
    {
        var resource = await _resourceRepository.GetByIdAsync(resourceId);
        if (resource == null)
        {
            return false;
        }

        if (isApproved)
        {
            resource.IsApproved = true;
            resource.ApprovedAt = DateTime.UtcNow;
            await _resourceRepository.UpdateAsync(resource);
            return true;
        }

        return await _resourceRepository.DeleteAsync(resourceId);
    }
}
