namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente l'historique des tentatives de connexion d'un utilisateur.
/// </summary>
public class LoginHistory
{
    /// <summary>Identifiant unique de l'enregistrement.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Adresse IP de la connexion.</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>User-Agent du client.</summary>
    public string UserAgent { get; set; } = string.Empty;
    /// <summary>Fournisseur d'authentification utilisé.</summary>
    public string? Provider { get; set; }
    /// <summary>Indique si la connexion a réussi.</summary>
    public bool Success { get; set; }
    /// <summary>Raison de l'échec éventuel.</summary>
    public string? FailureReason { get; set; }
    /// <summary>Date de la tentative de connexion.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
