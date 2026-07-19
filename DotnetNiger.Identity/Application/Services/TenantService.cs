using Microsoft.EntityFrameworkCore;
using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Common.Exceptions;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Application.Services;

public class TenantService
{
    private readonly IdentityDbContext _db;

    public TenantService(IdentityDbContext db) => _db = db;

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == request.Slug.ToLowerInvariant()))
            throw new SlugAlreadyExistsException(request.Slug);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(), Name = request.Name,
            Slug = request.Slug.ToLowerInvariant(),
            Description = request.Description, IsActive = true
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        return MapToResponse(tenant);
    }

    public async Task<PaginatedResponse<TenantResponse>> GetAllAsync(PaginationQuery pagination)
    {
        var query = _db.Tenants.AsNoTracking().OrderBy(t => t.Name);
        var total = await query.CountAsync();
        var items = await query
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.Description, t.IsActive, t.CreatedAt))
            .ToListAsync();
        return new PaginatedResponse<TenantResponse>(items, total, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<TenantResponse?> GetByIdAsync(Guid id)
    {
        var tenant = await _db.Tenants.FindAsync(id);
        return tenant == null ? null : MapToResponse(tenant);
    }

    public async Task<TenantResponse?> GetBySlugAsync(string slug)
    {
        var tenant = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
        return tenant == null ? null : MapToResponse(tenant);
    }

    public async Task<TenantResponse> UpdateAsync(Guid id, UpdateTenantRequest request)
    {
        var tenant = await _db.Tenants.FindAsync(id)
            ?? throw new KeyNotFoundException("Tenant non trouvé");
        if (request.Name != null) tenant.Name = request.Name;
        if (request.Description != null) tenant.Description = request.Description;
        if (request.IsActive.HasValue) tenant.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync();
        return MapToResponse(tenant);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _db.Tenants.FindAsync(id);
        if (tenant != null)
        {
            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
        }
    }

    private static TenantResponse MapToResponse(Tenant t) =>
        new(t.Id, t.Name, t.Slug, t.Description, t.IsActive, t.CreatedAt);
}
