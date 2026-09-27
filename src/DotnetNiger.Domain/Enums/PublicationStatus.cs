namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Cycle de vie editorial commun aux articles, ressources et projets.
/// </summary>
public enum PublicationStatus
{
    /// <summary>Redige, invisible du public.</summary>
    Draft = 0,

    /// <summary>Visible du public.</summary>
    Published = 1,

    /// <summary>Retire du site mais conserve l'historique et les commentaires lies.</summary>
    Archived = 2,
}
