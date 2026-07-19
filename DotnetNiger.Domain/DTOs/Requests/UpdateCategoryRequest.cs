namespace DotnetNiger.Domain.DTOs.Requests;

public record UpdateCategoryRequest(
    string? Name = null,
    string? Slug = null,
    string? Description = null,
    string? IconUrl = null);
