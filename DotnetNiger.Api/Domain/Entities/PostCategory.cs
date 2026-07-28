namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Table de liaison entre les publications et les catégories.
/// </summary>
public class PostCategory
{
    /// <summary>Identifiant de la publication.</summary>
    public Guid PostId { get; set; }
    /// <summary>Navigation vers la publication.</summary>
    public Post Post { get; set; } = null!;
    /// <summary>Identifiant de la catégorie.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Navigation vers la catégorie.</summary>
    public Category Category { get; set; } = null!;
}
