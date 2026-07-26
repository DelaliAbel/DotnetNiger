namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète d'un article.</summary>
public record PostResponse(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string Excerpt,
    string CoverImageUrl,
    Guid AuthorId,
    string Status,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string AuthorName,
    string AuthorAvatar,
    string PostType,
    int ViewCount,
    List<CategoryResponse> Categories,
    List<TagResponse> Tags);
