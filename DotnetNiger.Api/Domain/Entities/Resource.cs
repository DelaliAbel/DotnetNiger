namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente une ressource partagée (tutoriel, documentation, outil, etc.).
/// </summary>
public class Resource
{
    /// <summary>Identifiant unique de la ressource.</summary>
    public Guid Id { get; set; }
    /// <summary>Titre de la ressource.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description de la ressource.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL principale de la ressource.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>URL de téléchargement optionnelle.</summary>
    public string? DownloadUrl { get; set; }
    /// <summary>URL de la miniature.</summary>
    public string? ThumbnailUrl { get; set; }
    /// <summary>Type de la ressource (vidéo, PDF, lien, etc.).</summary>
    public string ResourceType { get; set; } = string.Empty;
    /// <summary>Niveau de difficulté (débutant, intermédiaire, avancé).</summary>
    public string Level { get; set; } = string.Empty;
    /// <summary>Statut de publication.</summary>
    public ResourceStatus Status { get; set; } = ResourceStatus.Draft;
    /// <summary>Identifiant du créateur.</summary>
    public Guid CreatedBy { get; set; }
    /// <summary>Identifiant de l'auteur.</summary>
    public Guid AuthorId { get; set; }
    /// <summary>Nombre de vues.</summary>
    public int ViewCount { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation vers l'auteur.</summary>
    public ApplicationUser? Author { get; set; }
    /// <summary>Catégories associées à la ressource.</summary>
    public ICollection<ResourceCategory> ResourceCategories { get; set; } = [];
    /// <summary>Tags associés à la ressource.</summary>
    public ICollection<ResourceTag> ResourceTags { get; set; } = [];
}

public enum ResourceStatus
{
    Draft,
    PendingReview,
    Published,
    Archived
}
