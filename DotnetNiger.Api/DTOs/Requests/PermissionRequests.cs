using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'une permission.</summary>
public record CreatePermissionRequest(
    // <summary>Nom de la permission.</summary>
    [Required] string Name,
    // <summary>Catégorie de la permission.</summary>
    [Required] string Category);

/// <summary>Requête d'attribution de permissions à un rôle.</summary>
public record AssignPermissionsRequest(
    // <summary>Identifiant du rôle cible.</summary>
    [Required] Guid RoleId,
    // <summary>Identifiants des permissions à attribuer.</summary>
    [Required] IList<Guid> PermissionIds);
