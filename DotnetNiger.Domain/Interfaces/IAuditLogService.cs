using DotnetNiger.Domain.Entities;

namespace DotnetNiger.Domain.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string entityType, Guid entityId, string action, string? description = null);

    Task LogAsync(AuditLog entry);
}
