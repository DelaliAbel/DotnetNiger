using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Identity.Application.DTOs.Requests;

/// <summary>Requête de création d'un utilisateur.</summary>
public record CreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    [Required] Guid TenantId,
    IList<string>? Roles = null);

/// <summary>Requête de mise à jour d'un utilisateur.</summary>
public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool? IsActive);

/// <summary>Requête de renvoi de l'email de confirmation.</summary>
public record ResendEmailConfirmationRequest(
    [Required][EmailAddress] string Email);

/// <summary>Requête de création d'un utilisateur par un administrateur.</summary>
public record AdminCreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password,
    [Required] string FirstName,
    string? LastName,
    [DotnetNiger.Identity.Api.Validation.ValidRole] string? Role);
