using DotnetNiger.Identity.Domain.Entities;

namespace DotnetNiger.Identity.Application.Services;

public interface IAuditLogService
{
    Task LogAsync(string entityType, Guid entityId, string action, string? description = null);

    Task LogAsync(AuditLog entry);
}
