namespace DotnetNiger.Common.DTOs.Responses;

/// <summary>Réponse contenant les données d'un rôle.</summary>
public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid TenantId,
    int UserCount);
