namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Circuit de traitement d'un signalement par la moderation.
/// </summary>
public enum ReportStatus
{
    /// <summary>Signalement recu, en attente de traitement.</summary>
    Open = 0,

    /// <summary>Un administrateur examine le contenu signale.</summary>
    UnderReview = 1,

    /// <summary>Signalement confirme, action de moderation appliquee.</summary>
    Resolved = 2,

    /// <summary>Signalement infonde, rejete par la moderation.</summary>
    Dismissed = 3,
}
