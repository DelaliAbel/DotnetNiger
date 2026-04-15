using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request to unsubscribe from newsletter
/// </summary>
public class UnsubscribeFromNewsletterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
