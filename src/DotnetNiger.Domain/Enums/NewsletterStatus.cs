namespace DotnetNiger.Domain.Enums;

/// <summary>
/// État d'un abonnement à la lettre d'information.
/// </summary>
/// <remarks>
/// Le double consentement n'est pas une formalité : une adresse inscrite sans confirmation
/// a été obtenue ailleurs, souvent d'une liste achetée ou d'une inscription abusive.
/// Tant que l'adresse n'a pas été confirmée par son titulaire, elle ne figure dans aucun
/// envoi et ne peut pas non plus avoir été volée pour s'y inscrire à la place de quelqu'un.
/// </remarks>
public enum NewsletterStatus
{
    /// <summary>Demande reçue, en attente de confirmation par son titulaire.</summary>
    Pending = 0,

    /// <summary>Abonnement confirmé, l'adresse reçoit la lettre.</summary>
    Confirmed = 1,

    /// <summary>Désabonnement demandé, l'adresse ne reçoit plus rien.</summary>
    Unsubscribed = 2,
}
