namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un service externe intégré à l'application.
/// </summary>
public class ExternalService
{
    /// <summary>Identifiant unique du service.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de la clé API associée.</summary>
    public Guid ApiKeyId { get; set; }
    /// <summary>Nom du service.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug du service.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description du service.</summary>
    public string? Description { get; set; }
    /// <summary>URL de base du service.</summary>
    public string BaseUrl { get; set; } = string.Empty;
    /// <summary>Point de contrôle de santé du service.</summary>
    public string HealthEndpoint { get; set; } = "/health";
    /// <summary>Indique si le service est actif.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Statut actuel du service.</summary>
    public ExternalServiceStatus Status { get; set; } = ExternalServiceStatus.Pending;
    /// <summary>Date du dernier contrôle de santé.</summary>
    public DateTime? LastHealthCheckAt { get; set; }
    /// <summary>Nombre d'échecs consécutifs de contrôle de santé.</summary>
    public int HealthCheckFailures { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
