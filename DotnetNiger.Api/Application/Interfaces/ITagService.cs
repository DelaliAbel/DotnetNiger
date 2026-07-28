using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des tags.</summary>
public interface ITagService
{
    /// <summary>Récupère tous les tags.</summary>
    Task<List<TagResponse>> GetAllAsync();
    /// <summary>Récupère un tag par identifiant.</summary>
    Task<TagResponse?> GetByIdAsync(Guid id);
    /// <summary>Récupère un tag par slug.</summary>
    Task<TagResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un tag.</summary>
    Task<TagResponse> CreateAsync(string name);
    /// <summary>Met à jour un tag.</summary>
    Task<TagResponse?> UpdateAsync(Guid id, string name);
    /// <summary>Supprime un tag.</summary>
    Task<bool> DeleteAsync(Guid id);
}
