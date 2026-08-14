using System.Threading;
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
    public async Task<SystemStatsResponse> GetSystemStatsAsync(CancellationToken ct = default)
    {
        var stats = await _cache.GetOrCreateAsync("SystemStats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var totalUsers = await _db.Users.CountAsync(ct);
            var totalRoles = await _db.Roles.CountAsync(ct);
            var totalPermissions = await _db.Permissions.CountAsync(ct);
            var totalRefreshTokens = await _db.RefreshTokens.CountAsync(ct);
            var totalServices = await _db.ExternalServices.CountAsync(ct);

            return new SystemStatsResponse(totalUsers, totalRoles, totalPermissions, totalRefreshTokens, totalServices);
        });
        return stats!;
    }

    /// <summary>Récupère les statistiques personnelles d'un utilisateur.</summary>
    public async Task<MyStatsResponse> GetMyStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var myEvents = await _db.Events.CountAsync(e => e.OrganizerId == userId, ct);
        var myPosts = await _db.Posts.CountAsync(p => p.AuthorId == userId, ct);
        var myResources = await _db.Resources.CountAsync(r => r.AuthorId == userId, ct);
        var myProjects = await _db.Projects.CountAsync(p => p.CreatedBy == userId, ct);

        return new MyStatsResponse(myEvents, myPosts, myResources, myProjects);
    }
}
