using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'un utilisateur (self-registration).</summary>
public record CreateUserRequest(
    // <summary>Adresse e-mail.</summary>
    [Required][EmailAddress] string Email,
    // <summary>Mot de passe.</summary>
    [Required] string Password,
    // <summary>Prénom.</summary>
    string? FirstName,
    // <summary>Nom de famille.</summary>
    string? LastName,
    // <summary>URL de l'avatar.</summary>
    string? AvatarUrl,
    // <summary>Rôles à attribuer.</summary>
    IList<string>? Roles = null);

/// <summary>Requête de mise à jour d'un utilisateur.</summary>
public record UpdateUserRequest(
    // <summary>Prénom.</summary>
    string? FirstName,
    // <summary>Nom de famille.</summary>
    string? LastName,
    // <summary>URL de l'avatar.</summary>
    string? AvatarUrl,
    // <summary>Indique si le compte est actif.</summary>
    bool? IsActive);

/// <summary>Requête de création d'un utilisateur par un administrateur.</summary>
public record AdminCreateUserRequest(
    // <summary>Adresse e-mail.</summary>
    [Required][EmailAddress] string Email,
    // <summary>Mot de passe.</summary>
    [Required] string Password,
    // <summary>Prénom.</summary>
    [Required] string FirstName,
    // <summary>Nom de famille.</summary>
    string? LastName,
    // <summary>Rôle à attribuer.</summary>
    string? Role);
