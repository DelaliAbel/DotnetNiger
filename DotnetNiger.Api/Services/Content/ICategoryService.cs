using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.Content;

/// <summary>Interface du service de gestion des catégories.</summary>
public interface ICategoryService
{
    /// <summary>Crée une catégorie.</summary>
    Task<CategoryResponse> CreateAsync(string name, string? description);
    /// <summary>Récupère toutes les catégories.</summary>
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync();
    /// <summary>Récupère une catégorie par identifiant.</summary>
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère une catégorie par slug.</summary>
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    /// <summary>Met à jour une catégorie.</summary>
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description);
    /// <summary>Supprime une catégorie.</summary>
    Task<bool> DeleteAsync(Guid id);
}
