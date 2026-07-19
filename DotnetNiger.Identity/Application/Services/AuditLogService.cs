using System.Security.Claims;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace DotnetNiger.Identity.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IdentityDbContext _db;
    private readonly TenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(IdentityDbContext db, TenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityType, Guid entityId, string action, string? description = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

        var entry = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            UserId = userId is not null && Guid.TryParse(userId, out var uid) ? uid : Guid.Empty,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task LogAsync(AuditLog entry)
    {
        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }
}
