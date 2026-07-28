using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de création d'un rôle.</summary>
public record CreateRoleRequest(
    // <summary>Nom du rôle.</summary>
    [Required] string Name,
    // <summary>Description du rôle.</summary>
    string? Description);

/// <summary>Requête de mise à jour d'un rôle.</summary>
public record UpdateRoleRequest(
    // <summary>Description du rôle.</summary>
    string? Description);
