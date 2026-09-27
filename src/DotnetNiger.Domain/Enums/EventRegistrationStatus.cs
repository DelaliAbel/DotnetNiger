namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Etat de l'inscription d'un membre a un evenement.
/// </summary>
public enum EventRegistrationStatus
{
    /// <summary>Place attribuee.</summary>
    Confirmed = 0,

    /// <summary>Annulee par le membre ou par un administrateur.</summary>
    Cancelled = 1,

    /// <summary>Evenement complet, le membre attend qu'une place se libere.</summary>
    Waitlisted = 2,
}
