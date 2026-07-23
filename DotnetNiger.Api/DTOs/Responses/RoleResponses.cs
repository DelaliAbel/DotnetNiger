namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'un rôle utilisateur.</summary>
public record RoleResponse(
    // <summary>Identifiant du rôle.</summary>
    Guid Id,
    // <summary>Nom du rôle.</summary>
    string Name,
    // <summary>Description du rôle.</summary>
    string? Description,
    // <summary>Nombre d'utilisateurs ayant ce rôle.</summary>
    int UserCount);
