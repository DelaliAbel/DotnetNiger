using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public record CreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    IList<string>? Roles = null);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool? IsActive);

public record ResendEmailConfirmationRequest(
    [Required][EmailAddress] string Email);

public record AdminCreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password,
    [Required] string FirstName,
    string? LastName,
    string? Role);
