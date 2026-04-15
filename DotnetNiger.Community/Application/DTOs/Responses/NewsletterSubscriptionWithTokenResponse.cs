namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response for newsletter subscription with verification token
/// </summary>
public class NewsletterSubscriptionWithTokenResponse : NewsletterSubscriptionResponse
{
    public string VerificationToken { get; set; } = string.Empty;
}
