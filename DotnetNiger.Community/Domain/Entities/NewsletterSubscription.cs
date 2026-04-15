namespace DotnetNiger.Community.Domain.Entities;

/// <summary>
/// Newsletter subscription entity - stores email addresses of newsletter subscribers
/// </summary>
public class NewsletterSubscription
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public string? VerificationToken { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }

    /// <summary>
    /// Optional: Link to community member if they created an account
    /// </summary>
    public Guid? MemberId { get; set; }
    public TeamMember? Member { get; set; }
}
