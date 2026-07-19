namespace DotnetNiger.Domain.DTOs.Responses;

public record MemberResponse(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string? Bio,
    string? Location,
    string? WebsiteUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
