namespace DotnetNiger.Api.Entities;

/// <summary>
/// Table de liaison entre les ressources et les tags.
/// </summary>
public class ResourceTag
{
    /// <summary>Identifiant de la ressource.</summary>
    public Guid ResourceId { get; set; }
    /// <summary>Navigation vers la ressource.</summary>
    public Resource Resource { get; set; } = null!;
    /// <summary>Identifiant du tag.</summary>
    public Guid TagId { get; set; }
    /// <summary>Navigation vers le tag.</summary>
    public Tag Tag { get; set; } = null!;
}
