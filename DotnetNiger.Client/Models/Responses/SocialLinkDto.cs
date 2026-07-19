namespace DotnetNiger.Client.Models.Responses;

public class SocialLinkDto
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icone => Platform switch
    {
        "GitHub"    => "fab fa-github",
        "LinkedIn"  => "fab fa-linkedin",
        "Portfolio" => "fas fa-briefcase",
        "Facebook"  => "fab fa-facebook",
        _           => "fas fa-link"
    };
}
