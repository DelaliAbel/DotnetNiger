using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête d'invitation d'un administrateur.</summary>
public record InviteAdminRequest(
    // <summary>Adresse e-mail de l'administrateur à inviter.</summary>
    [Required][EmailAddress] string Email,
    // <summary>Rôle à attribuer à l'administrateur.</summary>
    [Required] string Role);
