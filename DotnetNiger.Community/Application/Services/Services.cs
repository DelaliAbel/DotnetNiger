using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure.Repositories;

namespace DotnetNiger.Community.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<Post>> GetAllPublishedPostsAsync(int page = 1, int pageSize = 10)
    {
        return await _postRepository.GetPublishedPostsAsync(page, pageSize);
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        return await _postRepository.GetByIdAsync(id);
    }

    public async Task<Post?> GetPostBySlugAsync(string slug)
    {
        return await _postRepository.GetBySlugAsync(slug);
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        post.Id = Guid.NewGuid();
        post.CreatedAt = DateTime.UtcNow;
        return await _postRepository.AddAsync(post);
    }

    public async Task<Post> UpdatePostAsync(Post post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        return await _postRepository.UpdateAsync(post);
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        return await _postRepository.DeleteAsync(id);
    }
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.Id = Guid.NewGuid();
        return await _categoryRepository.AddAsync(category);
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        return await _categoryRepository.UpdateAsync(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }
}

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 10)
    {
        var allEvents = await _eventRepository.GetAllAsync();
        return allEvents.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _eventRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetUpcomingEventsAsync(limit);
    }

    public async Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10)
    {
        return await _eventRepository.GetPastEventsAsync(limit);
    }

    public async Task<Event> CreateEventAsync(Event @event)
    {
        @event.Id = Guid.NewGuid();
        @event.CreatedAt = DateTime.UtcNow;
        return await _eventRepository.AddAsync(@event);
    }

    public async Task<Event> UpdateEventAsync(Event @event)
    {
        @event.UpdatedAt = DateTime.UtcNow;
        return await _eventRepository.UpdateAsync(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        return await _eventRepository.DeleteAsync(id);
    }
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 10)
    {
        var allProjects = await _projectRepository.GetAllAsync();
        return allProjects.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _projectRepository.GetActivaProjectsAsync();
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        return await _projectRepository.AddAsync(project);
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        return await _projectRepository.UpdateAsync(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        return await _projectRepository.DeleteAsync(id);
    }
}

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;

    public ResourceService(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync(int page = 1, int pageSize = 10)
    {
        var allResources = await _resourceRepository.GetAllAsync();
        return allResources.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public async Task<Resource?> GetResourceByIdAsync(Guid id)
    {
        return await _resourceRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId)
    {
        return await _resourceRepository.GetByCategoryAsync(categoryId);
    }

    public async Task<Resource> CreateResourceAsync(Resource resource)
    {
        resource.Id = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        return await _resourceRepository.AddAsync(resource);
    }

    public async Task<Resource> UpdateResourceAsync(Resource resource)
    {
        return await _resourceRepository.UpdateAsync(resource);
    }

    public async Task<bool> DeleteResourceAsync(Guid id)
    {
        return await _resourceRepository.DeleteAsync(id);
    }
}

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _commentRepository.GetByPostIdAsync(postId);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;
        return await _commentRepository.AddAsync(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        return await _commentRepository.DeleteAsync(id);
    }
}

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _tagRepository.GetAllAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(Guid id)
    {
        return await _tagRepository.GetByIdAsync(id);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _tagRepository.GetByNameAsync(name);
    }

    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        tag.Id = Guid.NewGuid();
        return await _tagRepository.AddAsync(tag);
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        return await _tagRepository.DeleteAsync(id);
    }
}

public class PartnerService : IPartnerService
{
    private readonly IPartnerRepository _partnerRepository;

    public PartnerService(IPartnerRepository partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
    {
        return await _partnerRepository.GetAllAsync();
    }

    public async Task<Partner?> GetPartnerByIdAsync(Guid id)
    {
        return await _partnerRepository.GetByIdAsync(id);
    }

    public async Task<Partner> CreatePartnerAsync(Partner partner)
    {
        partner.Id = Guid.NewGuid();
        partner.CreatedAt = DateTime.UtcNow;
        return await _partnerRepository.AddAsync(partner);
    }

    public async Task<Partner> UpdatePartnerAsync(Partner partner)
    {
        return await _partnerRepository.UpdateAsync(partner);
    }

    public async Task<bool> DeletePartnerAsync(Guid id)
    {
        return await _partnerRepository.DeleteAsync(id);
    }
}

public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamMemberRepository _teamMemberRepository;

    public TeamMemberService(ITeamMemberRepository teamMemberRepository)
    {
        _teamMemberRepository = teamMemberRepository;
    }

    public async Task<IEnumerable<TeamMember>> GetActiveTeamMembersAsync()
    {
        return await _teamMemberRepository.GetActiveTeamMembersAsync();
    }

    public async Task<TeamMember?> GetTeamMemberByIdAsync(Guid id)
    {
        return await _teamMemberRepository.GetByIdAsync(id);
    }

    public async Task<TeamMember> CreateTeamMemberAsync(TeamMember teamMember)
    {
        teamMember.Id = Guid.NewGuid();
        teamMember.JoinedAt = DateTime.UtcNow;
        return await _teamMemberRepository.AddAsync(teamMember);
    }

    public async Task<TeamMember> UpdateTeamMemberAsync(TeamMember teamMember)
    {
        return await _teamMemberRepository.UpdateAsync(teamMember);
    }

    public async Task<bool> DeleteTeamMemberAsync(Guid id)
    {
        return await _teamMemberRepository.DeleteAsync(id);
    }
}
