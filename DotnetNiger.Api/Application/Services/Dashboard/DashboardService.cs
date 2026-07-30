using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Dashboard;

/// <summary>Service de tableau de bord fournissant les statistiques système et personnel.</summary>
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

    /// <summary>Récupère les statistiques globales du système (mis en cache 5 min).</summary>
    public async Task<SystemStatsResponse> GetSystemStatsAsync()
    {
        var stats = await _cache.GetOrCreateAsync("SystemStats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var totalUsers = await _db.Users.CountAsync();
            var totalRoles = await _db.Roles.CountAsync();
            var totalPermissions = await _db.Permissions.CountAsync();
            var totalRefreshTokens = await _db.RefreshTokens.CountAsync();
            var totalServices = await _db.ExternalServices.CountAsync();

            return new SystemStatsResponse(totalUsers, totalRoles, totalPermissions, totalRefreshTokens, totalServices);
        });
        return stats!;
    }

    /// <summary>Récupère l'historique des connexions avec pagination.</summary>
    public async Task<PaginatedResponse<LoginHistoryResponse>> GetLoginHistoryAsync(
        int page, int pageSize)
    {
        var query = _db.LoginHistories.AsNoTracking().OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoginHistoryResponse(
                l.Id, l.UserId, l.IpAddress, l.UserAgent,
                l.Provider, l.Success, l.FailureReason, l.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<LoginHistoryResponse>(items, total, page, pageSize);
    }

    /// <summary>Récupère les logs d'audit avec filtres et pagination.</summary>
    public async Task<PaginatedResponse<AuditLogResponse>> GetAuditLogsAsync(
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
            .Select(l => new AuditLogResponse(
                l.Id, l.UserId, l.EntityType, l.EntityId,
                l.Action, l.Description, l.IpAddress, l.CreatedAt))
            .ToListAsync();

        return new PaginatedResponse<AuditLogResponse>(items, total, page, pageSize);
    }

    /// <summary>Récupère les statistiques personnelles d'un utilisateur.</summary>
    public async Task<MyStatsResponse> GetMyStatsAsync(Guid userId)
    {
        var myEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId);
        var myPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId);
        var myResources = await _db.Resources.CountAsync(r => r.AuthorId == userId);
        var myProjects = await _db.Projects.CountAsync(p => p.CreatedBy == userId);

        return new MyStatsResponse(myEvents, myPosts, myResources, myProjects);
    }
}
