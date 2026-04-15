namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response for newsletter subscription
/// </summary>
public class NewsletterSubscriptionResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
