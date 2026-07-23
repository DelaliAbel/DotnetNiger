namespace DotnetNiger.Api.Entities;

/// <summary>
/// Table de liaison entre les ressources et les catégories.
/// </summary>
public class ResourceCategory
{
    /// <summary>Identifiant de la ressource.</summary>
    public Guid ResourceId { get; set; }
    /// <summary>Navigation vers la ressource.</summary>
    public Resource Resource { get; set; } = null!;
    /// <summary>Identifiant de la catégorie.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Navigation vers la catégorie.</summary>
    public Category Category { get; set; } = null!;
}
