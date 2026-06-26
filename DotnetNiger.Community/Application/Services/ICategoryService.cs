using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des catégories de contenu (articles, ressources).</summary>
public interface ICategoryService
{
    /// <summary>Liste de toutes les catégories triées par nom.</summary>
    Task<List<CategoryResponse>> GetAllAsync();
    /// <summary>Détail d'une catégorie par son identifiant.</summary>
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'une catégorie par son slug.</summary>
    Task<CategoryResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée une nouvelle catégorie.</summary>
    Task<CategoryResponse> CreateAsync(string name, string description);
    /// <summary>Modifie une catégorie existante.</summary>
    Task<CategoryResponse?> UpdateAsync(Guid id, string name, string description);
    /// <summary>Supprime une catégorie.</summary>
    Task<bool> DeleteAsync(Guid id);
}
