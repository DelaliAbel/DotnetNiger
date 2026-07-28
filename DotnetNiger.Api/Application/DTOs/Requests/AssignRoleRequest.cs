using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête d'attribution d'un rôle à un utilisateur.</summary>
public record AssignRoleRequest(
    // <summary>Nom du rôle à attribuer.</summary>
    [Required] string RoleName);
