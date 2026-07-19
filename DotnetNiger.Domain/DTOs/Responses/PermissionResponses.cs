namespace DotnetNiger.Domain.DTOs.Responses;

public record PermissionResponse(
    Guid Id,
    string Name,
    string Category);

public record PermissionGroupResponse(
    string Category,
    IList<PermissionResponse> Permissions);
