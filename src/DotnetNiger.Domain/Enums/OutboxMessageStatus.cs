namespace DotnetNiger.Domain.Enums;

/// <summary>
/// État d'un message en attente d'envoi.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>En attente, ou en cours de nouvelle tentative après un échec.</summary>
    Pending = 0,

    /// <summary>Expédié avec succès par le relais de fond.</summary>
    Sent = 1,

    /// <summary>
    /// Définitivement abandonné après épuisement des tentatives. Le corps est conservé
    /// afin que l'administration puisse comprendre l'échec, et non l'envoyer à la main.
    /// </summary>
    Failed = 2,
}
