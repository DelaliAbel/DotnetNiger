namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente une entrée du journal d'audit système.
/// </summary>
public class AuditLog
{
    /// <summary>Identifiant unique de l'entrée.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur ayant effectué l'action.</summary>
    public Guid UserId { get; set; }
    /// <summary>Type d'entité concernée.</summary>
    public string EntityType { get; set; } = string.Empty;
    /// <summary>Identifiant de l'entité concernée.</summary>
    public Guid EntityId { get; set; }
    /// <summary>Action effectuée (création, modification, suppression).</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>Description détaillée de l'action.</summary>
    public string? Description { get; set; }
    /// <summary>Adresse IP de l'utilisateur.</summary>
    public string? IpAddress { get; set; }
    /// <summary>Date de l'action.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
