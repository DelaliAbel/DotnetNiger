using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class TagService : ITagService
{
    private readonly ITagPersistence _tagRepository;
    private readonly ISlugGenerator _slugGenerator;

    public TagService(ITagPersistence tagRepository, ISlugGenerator slugGenerator)
    {
        _tagRepository = tagRepository;
        _slugGenerator = slugGenerator;
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
        tag.Slug = _slugGenerator.Generate(tag.Name);
        return await _tagRepository.AddAsync(tag);
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        return await _tagRepository.DeleteAsync(id);
    }
}
