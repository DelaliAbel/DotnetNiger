using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetAllPublishedPostsAsync(int page = 1, int pageSize = 10);
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<Post?> GetPostBySlugAsync(string slug);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(Post post);
    Task<bool> DeletePostAsync(Guid id);
}

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(Guid id);
}

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync(int page = 1, int pageSize = 10);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int limit = 10);
    Task<IEnumerable<Event>> GetPastEventsAsync(int limit = 10);
    Task<Event> CreateEventAsync(Event @event);
    Task<Event> UpdateEventAsync(Event @event);
    Task<bool> DeleteEventAsync(Guid id);
}

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync(int page = 1, int pageSize = 10);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(Guid id);
}

public interface IResourceService
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync(int page = 1, int pageSize = 10);
    Task<Resource?> GetResourceByIdAsync(Guid id);
    Task<IEnumerable<Resource>> GetResourcesByCategoryAsync(Guid categoryId);
    Task<Resource> CreateResourceAsync(Resource resource);
    Task<Resource> UpdateResourceAsync(Resource resource);
    Task<bool> DeleteResourceAsync(Guid id);
}

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(Guid id);
}

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(Guid id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> CreateTagAsync(Tag tag);
    Task<bool> DeleteTagAsync(Guid id);
}

public interface IPartnerService
{
    Task<IEnumerable<Partner>> GetAllPartnersAsync();
    Task<Partner?> GetPartnerByIdAsync(Guid id);
    Task<Partner> CreatePartnerAsync(Partner partner);
    Task<Partner> UpdatePartnerAsync(Partner partner);
    Task<bool> DeletePartnerAsync(Guid id);
}

public interface ITeamMemberService
{
    Task<IEnumerable<TeamMember>> GetActiveTeamMembersAsync();
    Task<TeamMember?> GetTeamMemberByIdAsync(Guid id);
    Task<TeamMember> CreateTeamMemberAsync(TeamMember teamMember);
    Task<TeamMember> UpdateTeamMemberAsync(TeamMember teamMember);
    Task<bool> DeleteTeamMemberAsync(Guid id);
}
