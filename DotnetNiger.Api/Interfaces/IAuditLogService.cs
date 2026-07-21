using DotnetNiger.Api.Entities;

namespace DotnetNiger.Api.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string entityType, Guid entityId, string action, string? description = null);

    Task LogAsync(AuditLog entry);
}
