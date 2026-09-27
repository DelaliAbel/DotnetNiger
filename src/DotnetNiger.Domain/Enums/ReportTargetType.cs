namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Nature de la cible d'un signalement.
/// </summary>
/// <remarks>
/// Un signalement est un enregistrement de moderation courte duree : il reference sa cible
/// par type et identifiant plutot que par une cle etrangere, ce qui evite d'ajouter une
/// colonne et une contrainte a chaque nouveau type de contenu modere.
/// </remarks>
public enum ReportTargetType
{
    /// <summary>Ciblee un article de blog.</summary>
    Article = 0,

    /// <summary>Ciblee un commentaire.</summary>
    Comment = 1,

    /// <summary>Ciblee une ressource.</summary>
    Resource = 2,

    /// <summary>Ciblee un evenement.</summary>
    CommunityEvent = 3,

    /// <summary>Ciblee un projet open source.</summary>
    Project = 4,

    /// <summary>Ciblee un membre.</summary>
    Member = 5,
}
