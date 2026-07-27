namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente une catégorie pour organiser les contenus.
/// </summary>
public class Category
{
    /// <summary>Identifiant unique de la catégorie.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom de la catégorie.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description de la catégorie.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL de l'icône de la catégorie.</summary>
    public string? IconUrl { get; set; }
    /// <summary>Nombre de publications dans cette catégorie.</summary>
    public int PostCount { get; set; }
    /// <summary>Indique si la catégorie est supprimée logiquement.</summary>
    public bool IsDeleted { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Publications associées à la catégorie.</summary>
    public ICollection<PostCategory> PostCategories { get; set; } = [];
    /// <summary>Ressources associées à la catégorie.</summary>
    public ICollection<ResourceCategory> ResourceCategories { get; set; } = [];
}
