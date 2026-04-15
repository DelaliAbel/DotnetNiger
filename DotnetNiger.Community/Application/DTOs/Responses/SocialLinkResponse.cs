namespace DotnetNiger.Community.Application.DTOs.Responses;

public class SocialLinkResponse
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
