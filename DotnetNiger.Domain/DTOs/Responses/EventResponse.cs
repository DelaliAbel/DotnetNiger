namespace DotnetNiger.Domain.DTOs.Responses;

public record EventResponse(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? CoverImageUrl,
    Guid OrganizerId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
