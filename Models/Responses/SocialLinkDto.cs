// DTO response Identity: SocialLinkDto
namespace DotnetNiger.UI.Models.Responses;

public class SocialLinkDto
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icone => Platform switch
    {
        "LinkedIn" => "fab fa-linkedin",
        "Twitter"  => "fab fa-twitter",
        "GitHub"   => "fab fa-github",
        "Facebook" => "fab fa-facebook",
        "YouTube"  => "fab fa-youtube",
        _ => "fas fa-link"
    };
}
