using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly DotnetNigerDbContext _db;

    public CategoryService(DotnetNigerDbContext db) => _db = db;

    public async Task<CategoryResponse> CreateAsync(string name, string? description)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    public async Task<PaginatedResponse<CategoryResponse>> GetAllAsync()
    {
        var query = _db.Categories.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return new PaginatedResponse<CategoryResponse>(
            items.Select(MapToResponse).ToList(), totalCount, 1, totalCount);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category == null ? null : MapToResponse(category);
    }

    public async Task<CategoryResponse?> GetBySlugAsync(string slug)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        return category == null ? null : MapToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = name;
        category.Description = description;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    private static CategoryResponse MapToResponse(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.IconUrl);
}
