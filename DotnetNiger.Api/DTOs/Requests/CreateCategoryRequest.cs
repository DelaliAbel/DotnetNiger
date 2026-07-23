using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'une catégorie.</summary>
public class CreateCategoryRequest
{
    /// <summary>Nom de la catégorie (max 100 caractères).</summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Slug URL de la catégorie.</summary>
    public string? Slug { get; set; }

    /// <summary>Description de la catégorie.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>URL de l'icône de la catégorie.</summary>
    public string? IconUrl { get; set; }
}
