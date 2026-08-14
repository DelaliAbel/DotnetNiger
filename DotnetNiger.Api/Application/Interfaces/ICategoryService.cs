using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des catégories.</summary>
public interface ICategoryService
{
    /// <summary>Crée une catégorie.</summary>
    Task<CategoryResponse> CreateAsync(string name, string? description, CancellationToken ct = default);
    /// <summary>Récupère toutes les catégories.</summary>
    Task<PaginatedResponse<CategoryResponse>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Récupère une catégorie par identifiant.</summary>
    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Récupère une catégorie par slug.</summary>
    Task<CategoryResponse?> GetBySlugAsync(string slug, CancellationToken ct = default);
    /// <summary>Met à jour une catégorie.</summary>
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string? description, CancellationToken ct = default);
    /// <summary>Supprime une catégorie.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
