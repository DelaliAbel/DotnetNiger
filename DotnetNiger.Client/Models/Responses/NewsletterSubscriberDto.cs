namespace DotnetNiger.Client.Models.Responses;

public class NewsletterSubscriberDto
{
    public string Email { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
    public bool IsConfirmed { get; set; }
}
