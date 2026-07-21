namespace DotnetNiger.Api.DTOs.Responses;

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    int UserCount);
