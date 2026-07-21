// DTO request Identity: AddSocialLinkRequest
using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class AddSocialLinkRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty; // Twitter, LinkedIn, GitHub, Facebook

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
}
