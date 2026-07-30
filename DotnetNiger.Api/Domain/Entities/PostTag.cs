namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Table de liaison entre les publications et les tags.
/// </summary>
public class PostTag
{
    /// <summary>Identifiant de la publication.</summary>
    public Guid PostId { get; set; }
    /// <summary>Navigation vers la publication.</summary>
    public Post Post { get; set; } = null!;
    /// <summary>Identifiant du tag.</summary>
    public Guid TagId { get; set; }
    /// <summary>Navigation vers le tag.</summary>
    public Tag Tag { get; set; } = null!;
}
