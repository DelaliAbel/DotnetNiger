namespace DotnetNiger.Api.DTOs.Responses;

public record UserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedAt,
    IList<string> Roles);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    IList<string> Roles);
