using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Dtos.Requests;

public class AddSocialLinkRequest
{
    [Required]
    public string Platform { get; set; } = string.Empty;

    [Required, Url]
    public string Url { get; set; } = string.Empty;
}
