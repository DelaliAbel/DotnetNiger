using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(Guid id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> CreateTagAsync(Tag tag);
    Task<bool> DeleteTagAsync(Guid id);
}

