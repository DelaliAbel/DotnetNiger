using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

public class CreateCategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }
}
