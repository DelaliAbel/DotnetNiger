using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un projet.</summary>
public class CreateProjectRequest
{
    /// <summary>Titre du projet (max 200 caractères).</summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Description du projet.</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>URL du projet.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>URL du dépôt GitHub.</summary>
    public string GithubUrl { get; set; } = string.Empty;
    /// <summary>URL de l'image du projet.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>Technologies utilisées.</summary>
    public string Technologies { get; set; } = string.Empty;
    /// <summary>Statut du projet (active, archived, etc.).</summary>
    public string Status { get; set; } = "active";
    /// <summary>Indique si le projet est mis en avant.</summary>
    public bool IsFeatured { get; set; }
    /// <summary>Indique si le projet est publié.</summary>
    public bool IsPublished { get; set; }
}

/// <summary>Requête de mise à jour d'un projet.</summary>
public class UpdateProjectRequest
{
    /// <summary>Titre du projet (max 200 caractères).</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Description du projet.</summary>
    public string? Description { get; set; }

    /// <summary>URL du projet.</summary>
    public string? Url { get; set; }

    /// <summary>URL du dépôt GitHub.</summary>
    public string? GithubUrl { get; set; }

    /// <summary>URL de l'image du projet.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Technologies utilisées.</summary>
    public string? Technologies { get; set; }

    /// <summary>Statut du projet.</summary>
    public string? Status { get; set; }

    /// <summary>Indique si le projet est mis en avant.</summary>
    public bool? IsFeatured { get; set; }

    /// <summary>Indique si le projet est publié.</summary>
    public bool? IsPublished { get; set; }
}
