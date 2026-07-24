namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un événement.</summary>
public record EventResponse(
    // <summary>Identifiant de l'événement.</summary>
    Guid Id,
    // <summary>Titre de l'événement.</summary>
    string Title,
    // <summary>Slug URL de l'événement.</summary>
    string Slug,
    // <summary>Description de l'événement.</summary>
    string Description,
    // <summary>Date et heure de début.</summary>
    DateTime StartDate,
    // <summary>Date et heure de fin.</summary>
    DateTime EndDate,
    // <summary>Lieu de l'événement.</summary>
    string Location,
    // <summary>URL de l'image de couverture.</summary>
    string? CoverImageUrl,
    // <summary>Identifiant du créateur.</summary>
    Guid CreatedBy,
    // <summary>Statut de l'événement.</summary>
    string Status,
    // <summary>Indique si l'événement est publié.</summary>
    bool IsPublished,
    // <summary>Date de création.</summary>
    DateTime CreatedAt,
    // <summary>Date de dernière mise à jour.</summary>
    DateTime UpdatedAt);
