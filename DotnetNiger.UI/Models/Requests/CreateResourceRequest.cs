using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class CreateResourceRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string ResourceType { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = string.Empty;

    public List<Guid> CategoryIds { get; set; } = new();
}
