using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request to verify newsletter subscription
/// </summary>
public class VerifyNewsletterSubscriptionRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
