namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente une demande de suppression de compte utilisateur.
/// </summary>
public class AccountDeletionRequest
{
    /// <summary>Identifiant unique de la demande.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur demandeur.</summary>
    public Guid UserId { get; set; }
    /// <summary>Date de la demande.</summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date prévue de la suppression.</summary>
    public DateTime ScheduledFor { get; set; }
    /// <summary>Date d'annulation de la demande.</summary>
    public DateTime? CancelledAt { get; set; }
    /// <summary>Indique si la demande a été traitée.</summary>
    public bool IsProcessed { get; set; }

    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
}
