namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un projet.</summary>
public class ProjectResponse
{
    /// <summary>Identifiant du projet.</summary>
    public Guid Id { get; set; }
    /// <summary>Titre du projet.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Slug URL du projet.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description du projet.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du projet.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>URL du dépôt GitHub.</summary>
    public string GithubUrl { get; set; } = string.Empty;
    /// <summary>URL de l'image du projet.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>Technologies utilisées.</summary>
    public string Technologies { get; set; } = string.Empty;
    /// <summary>Statut du projet.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Identifiant du créateur.</summary>
    public Guid CreatedBy { get; set; }
    /// <summary>Nom de l'auteur du projet.</summary>
    public string AuthorName { get; set; } = string.Empty;
    /// <summary>Indique si le projet est mis en avant.</summary>
    public bool IsFeatured { get; set; }
    /// <summary>Indique si le projet est publié.</summary>
    public bool IsPublished { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; }
}
