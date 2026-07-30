namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un tag pour catégoriser les contenus.
/// </summary>
public class Tag
{
    /// <summary>Identifiant unique du tag.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom du tag.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Nombre d'utilisations du tag.</summary>
    public int UsageCount { get; set; }

    /// <summary>Publications associées au tag.</summary>
    public ICollection<PostTag> PostTags { get; set; } = [];
    /// <summary>Événements associés au tag.</summary>
    public ICollection<EventTag> EventTags { get; set; } = [];
    /// <summary>Ressources associées au tag.</summary>
    public ICollection<ResourceTag> ResourceTags { get; set; } = [];
}
