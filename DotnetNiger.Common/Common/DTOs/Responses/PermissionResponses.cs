namespace DotnetNiger.Common.DTOs.Responses;

/// <summary>Réponse contenant les données d'une permission.</summary>
public record PermissionResponse(
    Guid Id,
    string Name,
    string Category,
    Guid TenantId);

/// <summary>Réponse contenant un groupe de permissions par catégorie.</summary>
public record PermissionGroupResponse(
    string Category,
    IList<PermissionResponse> Permissions);
