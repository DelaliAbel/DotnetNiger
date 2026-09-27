namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Cycle de vie d'un commentaire. Conformement a la decision de moderation, un commentaire
/// publie par un membre connecte est visible immediatement ; le signalement declenche une
/// re-examen a posteriori.
/// </summary>
public enum CommentStatus
{
    /// <summary>Visible du public.</summary>
    Published = 0,

    /// <summary>Retenu par la moderation, en attente de re-examen.</summary>
    PendingReview = 1,

    /// <summary>Masque par un administrateur, reste consultable par la moderation.</summary>
    Hidden = 2,

    /// <summary>Supprime par un administrateur.</summary>
    Deleted = 3,
}
