using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

public class DashboardService
{
    private readonly DotnetNigerDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DashboardService(DotnetNigerDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<object> GetSystemStatsAsync()
    {
        var stats = await _cache.GetOrCreateAsync("SystemStats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync();
            var totalRoles = await _db.Roles.IgnoreQueryFilters().CountAsync();
            var totalPermissions = await _db.Permissions.IgnoreQueryFilters().CountAsync();
            var totalApiKeys = await _db.ApiKeys.IgnoreQueryFilters().CountAsync();
            var totalServices = await _db.ExternalServices.IgnoreQueryFilters().CountAsync();
            var totalClients = await _db.OAuthClients.IgnoreQueryFilters().CountAsync();

            return new
            {
                totalUsers,
                totalRoles,
                totalPermissions,
                totalApiKeys,
                totalServices,
                totalClients
            };
        });
        return stats!;
    }

    public async Task<PaginatedResponse<LoginHistory>> GetLoginHistoryAsync(
        int page, int pageSize)
    {
        var query = _db.LoginHistories.AsNoTracking().OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<LoginHistory>(items, total, page, pageSize);
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

    public async Task<object> GetMyStatsAsync(Guid userId)
    {
        var myEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId);
        var myPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId);
        var myResources = await _db.Resources.CountAsync(r => r.AuthorId == userId);
        var myProjects = await _db.Projects.CountAsync(p => p.CreatedBy == userId);

        return new
        {
            eventsCount = myEvents,
            blogsCount = myPosts,
            resourcesCount = myResources,
            projectsCount = myProjects
        };
    }
}
