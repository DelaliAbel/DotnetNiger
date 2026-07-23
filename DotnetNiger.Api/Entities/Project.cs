namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un projet communautaire du DotnetNiger.
/// </summary>
public class Project
{
    /// <summary>Identifiant unique du projet.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom du projet.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Titre du projet.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description du projet.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du site du projet.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>URL du dépôt GitHub.</summary>
    public string GithubUrl { get; set; } = string.Empty;
    /// <summary>URL alternative du dépôt.</summary>
    public string? RepositoryUrl { get; set; }
    /// <summary>URL de la démo.</summary>
    public string? DemoUrl { get; set; }
    /// <summary>URL de l'image du projet.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>Technologies utilisées.</summary>
    public string Technologies { get; set; } = string.Empty;
    /// <summary>Statut du projet (active, archived, etc.).</summary>
    public string Status { get; set; } = "active";
    /// <summary>Identifiant du créateur.</summary>
    public Guid CreatedBy { get; set; }
    /// <summary>Nom de l'auteur.</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>Indique si le projet est mis en avant.</summary>
    public bool IsFeatured { get; set; }
    /// <summary>Indique si le projet est publié.</summary>
    public bool IsPublished { get; set; }
    /// <summary>Indique si le projet est supprimé logiquement.</summary>
    public bool IsDeleted { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
