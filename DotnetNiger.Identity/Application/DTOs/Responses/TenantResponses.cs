namespace DotnetNiger.Identity.Application.DTOs.Responses;

/// <summary>Réponse contenant les données d'un tenant.</summary>
public record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
