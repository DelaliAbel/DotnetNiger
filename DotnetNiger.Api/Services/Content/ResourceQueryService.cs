using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.Content;

public class ResourceQueryService : IResourceQueryService
{
    private readonly DotnetNigerDbContext _db;

    public ResourceQueryService(DotnetNigerDbContext db) => _db = db;

    public async Task<PaginatedResponse<ResourceResponse>> GetAllAsync(
        string? resourceType, string? level, string? query,
        string? tag, Guid? categoryId, int page, int pageSize, Guid? after = null, Guid? authorId = null)
    {
        var q = _db.Resources.AsNoTracking();

        if (authorId.HasValue) q = q.Where(r => r.AuthorId == authorId.Value);
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(r => r.Title.Contains(query) || (r.Description != null && r.Description.Contains(query)));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ResourceResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    public async Task<ResourceResponse?> GetByIdAsync(Guid id)
    {
        var r = await _db.Resources.FindAsync(id);
        return r == null ? null : MapToResponse(r);
    }

    public async Task<ResourceResponse?> GetBySlugAsync(string slug)
    {
        var r = await _db.Resources.FirstOrDefaultAsync(res => res.Slug == slug);
        return r == null ? null : MapToResponse(r);
    }

    public async Task<List<string>> GetResourceTypesAsync()
    {
        return await _db.Resources.AsNoTracking()
            .Where(r => r.ResourceType != null)
            .Select(r => r.ResourceType!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<List<string>> GetLevelsAsync()
    {
        return await _db.Resources.AsNoTracking()
            .Where(r => r.Level != null)
            .Select(r => r.Level!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }

    private static ResourceResponse MapToResponse(Resource r) =>
        new(r.Id, r.Title, r.Slug, r.Description, r.Url, r.DownloadUrl, r.ThumbnailUrl,
            r.AuthorId, r.Status.ToString(), r.CreatedAt, r.UpdatedAt);
}
