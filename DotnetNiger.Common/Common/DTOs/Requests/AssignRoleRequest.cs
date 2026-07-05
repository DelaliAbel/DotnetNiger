using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Common.DTOs.Requests;

/// <summary>Requête d'attribution d'un rôle à un utilisateur.</summary>
public record AssignRoleRequest(
    [Required] string RoleName);
