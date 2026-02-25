using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class CreateProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Url]
    public string GitHubUrl { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
}
