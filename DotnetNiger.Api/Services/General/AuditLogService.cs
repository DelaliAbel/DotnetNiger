using System.Security.Claims;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Interfaces;
using DotnetNiger.Api.Data;
using Microsoft.AspNetCore.Http;

namespace DotnetNiger.Api.Services.General;

public class AuditLogService : IAuditLogService
{
    private readonly DotnetNigerDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(DotnetNigerDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
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
