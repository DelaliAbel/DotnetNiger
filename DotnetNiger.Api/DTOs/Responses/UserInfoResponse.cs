namespace DotnetNiger.Api.DTOs.Responses;

public record UserInfoResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool IsActive,
    IList<string> Roles,
    IList<string> Permissions,
    bool RememberMe = false);
