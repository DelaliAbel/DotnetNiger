namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'une permission.</summary>
public record PermissionResponse(
    // <summary>Identifiant de la permission.</summary>
    Guid Id,
    // <summary>Nom de la permission.</summary>
    string Name,
    // <summary>Catégorie de la permission.</summary>
    string Category);

/// <summary>Réponse d'un groupe de permissions.</summary>
public record PermissionGroupResponse(
    // <summary>Nom de la catégorie.</summary>
    string Category,
    // <summary>Liste des permissions du groupe.</summary>
    IList<PermissionResponse> Permissions);
