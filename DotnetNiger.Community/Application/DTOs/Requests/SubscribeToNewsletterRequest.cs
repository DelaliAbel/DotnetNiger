using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request to subscribe to the newsletter
/// </summary>
public class SubscribeToNewsletterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
