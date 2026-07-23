namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'une catégorie.</summary>
public record CategoryResponse(
    // <summary>Identifiant de la catégorie.</summary>
    Guid Id,
    // <summary>Nom de la catégorie.</summary>
    string Name,
    // <summary>Slug URL de la catégorie.</summary>
    string Slug,
    // <summary>Description de la catégorie.</summary>
    string Description,
    // <summary>URL de l'icône de la catégorie.</summary>
    string? IconUrl = null,
    // <summary>Nombre d'articles dans la catégorie.</summary>
    int PostCount = 0);
