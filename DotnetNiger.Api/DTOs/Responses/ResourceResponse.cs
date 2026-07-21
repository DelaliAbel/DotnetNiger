namespace DotnetNiger.Api.DTOs.Responses;

public record ResourceResponse(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string Url,
    string? DownloadUrl,
    string? ThumbnailUrl,
    Guid AuthorId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
