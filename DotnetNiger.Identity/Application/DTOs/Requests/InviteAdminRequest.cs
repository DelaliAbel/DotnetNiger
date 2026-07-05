using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête d'invitation d'un administrateur.</summary>
public record InviteAdminRequest(
    [Required][EmailAddress] string Email,
    [Required] string Role);
