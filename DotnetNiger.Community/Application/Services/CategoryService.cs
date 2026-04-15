using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryPersistence _categoryRepository;
    private readonly ISlugGenerator _slugGenerator;

    public CategoryService(ICategoryPersistence categoryRepository, ISlugGenerator slugGenerator)
    {
        _categoryRepository = categoryRepository;
        _slugGenerator = slugGenerator;
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
        category.Slug = _slugGenerator.Generate(category.Name);
        return await _categoryRepository.AddAsync(category);
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        category.Slug = _slugGenerator.Generate(category.Name);
        return await _categoryRepository.UpdateAsync(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }
}
