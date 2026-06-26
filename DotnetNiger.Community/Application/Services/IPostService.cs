using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des articles de blog.</summary>
public interface IPostService
{
    /// <summary>Recherche paginée avec filtres (publication, catégorie, tag, mot-clé).</summary>
    Task<PaginatedResponse<PostResponse>> GetAllAsync(string? published, string? category, string? tag, string? query, int page = 1, int pageSize = 10, Guid? after = null);
    /// <summary>Détail d'un article avec catégories et tags.</summary>
    Task<PostResponse?> GetByIdAsync(Guid id);
    /// <summary>Détail d'un article par son slug.</summary>
    Task<PostResponse?> GetBySlugAsync(string slug);
    /// <summary>Crée un article relié à ses catégories et tags.</summary>
    Task<PostResponse> CreateAsync(CreatePostRequest request, Guid authorId, string authorName);
    /// <summary>Modifie un article (vérifie le propriétaire ou le rôle admin).</summary>
    Task<PostResponse?> UpdateAsync(Guid id, UpdatePostRequest request, Guid userId, bool isAdmin);
    /// <summary>Publie un article.</summary>
    Task<PostResponse?> PublishAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Dépublie un article.</summary>
    Task<PostResponse?> UnpublishAsync(Guid id, Guid userId, bool isAdmin);
    /// <summary>Incémente le compteur de vues de l'article.</summary>
    Task<PostResponse?> IncrementViewCountAsync(Guid id);
    /// <summary>Supprime définitivement un article.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);
}
