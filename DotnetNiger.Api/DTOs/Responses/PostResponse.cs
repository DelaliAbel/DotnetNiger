namespace DotnetNiger.Api.DTOs.Responses;

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
    DateTime UpdatedAt);
