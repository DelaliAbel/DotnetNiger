using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Domain.DTOs.Requests;

public class AddSocialLinkRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty;

    [Required, Url]
    public string Url { get; set; } = string.Empty;
}
