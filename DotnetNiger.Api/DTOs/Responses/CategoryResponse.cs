namespace DotnetNiger.Api.DTOs.Responses;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string? IconUrl = null,
    int PostCount = 0);
