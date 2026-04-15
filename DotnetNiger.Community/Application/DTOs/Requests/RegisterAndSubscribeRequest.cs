using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request to register/create account and subscribe to newsletter
/// </summary>
public class RegisterAndSubscribeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string FullName { get; set; } = string.Empty;

    public bool SubscribeToNewsletter { get; set; } = true;
}
