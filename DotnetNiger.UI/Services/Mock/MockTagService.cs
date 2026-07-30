using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock;

public class MockTagService : ITagService
{
    private readonly List<TagDto> _tags = new()
    {
        new TagDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Blazor", Slug = "blazor" },
        new TagDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "C#13", Slug = "csharp13" },
        new TagDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Aspire", Slug = "aspire" },
        new TagDto { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "WASM", Slug = "wasm" }
    };

    public Task<List<TagDto>> GetAllAsync() => Task.FromResult(_tags.ToList());

    public Task<TagDto?> GetByIdAsync(Guid id) =>
        Task.FromResult(_tags.FirstOrDefault(t => t.Id == id));

    public Task<TagDto?> GetBySlugAsync(string slug) =>
        Task.FromResult(_tags.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)));

    public Task<TagDto?> CreateAsync(string name)
    {
        var tag = new TagDto
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLowerInvariant().Replace(" ", "-")
        };
        _tags.Add(tag);
        return Task.FromResult<TagDto?>(tag);
    }

    public Task<TagDto?> UpdateAsync(Guid id, string name)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == id);
        if (tag is null) return Task.FromResult<TagDto?>(null);
        tag.Name = name;
        return Task.FromResult<TagDto?>(tag);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _tags.RemoveAll(t => t.Id == id);
        return Task.FromResult(removed > 0);
    }
}
