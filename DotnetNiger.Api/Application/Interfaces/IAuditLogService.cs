using System.Threading;
using DotnetNiger.Api.Domain.Entities;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>
/// Interface du service de journalisation d'audit.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Enregistre une entrée d'audit pour une entité donnée.
    /// </summary>
    Task LogAsync(string entityType, Guid entityId, string action, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Enregistre une entrée d'audit pré-construite.
    /// </summary>
    Task LogAsync(AuditLog entry, CancellationToken ct = default);
}
