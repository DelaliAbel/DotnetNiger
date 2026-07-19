using DotnetNiger.Community.Application.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des tags (étiquettes de contenu).</summary>
public interface ITagService
{
    /// <summary>Liste de tous les tags triés par nom.</summary>
    Task<List<TagResponse>> GetAllAsync();
    /// <summary>Détail d'un tag par son identifiant.</summary>
    Task<TagResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'un tag par son slug.</summary>
    Task<TagResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un nouveau tag.</summary>
    Task<TagResponse> CreateAsync(string name);
    /// <summary>Modifie un tag existant.</summary>
    Task<TagResponse?> UpdateAsync(Guid id, string name);
    /// <summary>Supprime un tag.</summary>
    Task<bool> DeleteAsync(Guid id);
}
