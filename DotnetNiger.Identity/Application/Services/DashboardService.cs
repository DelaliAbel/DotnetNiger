using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Application.Services;

public class DashboardService
{
    private readonly IdentityDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DashboardService(IdentityDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<object> GetSystemStatsAsync()
    {
        var stats = await _cache.GetOrCreateAsync("SystemStats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var totalTenants = await _db.Tenants.CountAsync();
            var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
            var totalRoles = await _db.Roles.IgnoreQueryFilters().CountAsync();
            var totalPermissions = await _db.Permissions.IgnoreQueryFilters().CountAsync();
            var totalApiKeys = await _db.TenantApiKeys.IgnoreQueryFilters().CountAsync();
            var totalServices = await _db.ExternalServices.IgnoreQueryFilters().CountAsync();
            var totalClients = await _db.TenantClients.IgnoreQueryFilters().CountAsync();

            return new
            {
                totalTenants,
                totalUsers,
                totalRoles,
                totalPermissions,
                totalApiKeys,
                totalServices,
                totalClients,
                activeTenants = await _db.Tenants.CountAsync(t => t.IsActive)
            };
        });
        return stats!;
    }

    public async Task<object> GetTenantLoginHistoryAsync(Guid tenantId, int page, int pageSize)
    {
        var query = _db.LoginHistories
            .Where(h => _db.Users.Any(u => u.Id == h.UserId && u.TenantId == tenantId));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_db.Users.AsNoTracking(),
                h => h.UserId,
                u => u.Id,
                (h, u) => new
                {
                    h.Id,
                    h.UserId,
                    Email = u.Email,
                    h.IpAddress,
                    h.UserAgent,
                    h.Provider,
                    h.Success,
                    h.FailureReason,
                    h.CreatedAt
                })
            .ToListAsync();

        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PaginatedResponse<AuditLog>> GetAuditLogsAsync(
        int page, int pageSize,
        string? entityType = null, string? action = null,
        DateTime? from = null, DateTime? to = null)
    {
        var query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<AuditLog>(items, total, page, pageSize);
    }
}
