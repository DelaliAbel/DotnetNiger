using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;

namespace DotnetNiger.UI.Services.Mock;

public class MockCategoryService : ICategoryService
{
    private readonly List<CategoryDto> _categories = new()
    {
        new CategoryDto { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Blazor", Slug = "blazor", Description = "Articles sur Blazor", PostCount = 5 },
        new CategoryDto { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "C#", Slug = "csharp", Description = "Articles sur C#", PostCount = 8 },
        new CategoryDto { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = ".NET", Slug = "dotnet", Description = "Articles sur .NET", PostCount = 12 },
        new CategoryDto { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Cloud", Slug = "cloud", Description = "Articles sur le cloud", PostCount = 3 }
    };

    public Task<List<CategoryDto>> GetAllAsync() => Task.FromResult(_categories.ToList());

    public Task<CategoryDto?> GetByIdAsync(Guid id) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task<CategoryDto?> GetBySlugAsync(string slug) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)));

    public Task<CategoryDto?> CreateAsync(string name, string description)
    {
        var category = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLowerInvariant().Replace(" ", "-"),
            Description = description
        };
        _categories.Add(category);
        return Task.FromResult<CategoryDto?>(category);
    }

    public Task<CategoryDto?> UpdateAsync(Guid id, string name, string description)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category is null) return Task.FromResult<CategoryDto?>(null);
        category.Name = name;
        category.Description = description;
        return Task.FromResult<CategoryDto?>(category);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _categories.RemoveAll(c => c.Id == id);
        return Task.FromResult(removed > 0);
    }
}
