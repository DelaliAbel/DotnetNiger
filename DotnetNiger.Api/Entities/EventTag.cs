namespace DotnetNiger.Api.Entities;

/// <summary>
/// Table de liaison entre les événements et les tags.
/// </summary>
public class EventTag
{
    /// <summary>Identifiant de l'événement.</summary>
    public Guid EventId { get; set; }
    /// <summary>Navigation vers l'événement.</summary>
    public Event Event { get; set; } = null!;
    /// <summary>Identifiant du tag.</summary>
    public Guid TagId { get; set; }
    /// <summary>Navigation vers le tag.</summary>
    public Tag Tag { get; set; } = null!;
}
