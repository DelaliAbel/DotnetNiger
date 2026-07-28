using System.Security.Claims;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace DotnetNiger.Api.Application.Services.Settings;

/// <summary>Service de journalisation des actions d'audit (qui a fait quoi et quand).</summary>
public class AuditLogService : IAuditLogService
{
    private readonly DotnetNigerDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(DotnetNigerDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Enregistre une entrée d'audit avec les informations de contexte HTTP.</summary>
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

    /// <summary>Enregistre une entrée d'audit pré-construite.</summary>
    public async Task LogAsync(AuditLog entry)
    {
        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }
}
