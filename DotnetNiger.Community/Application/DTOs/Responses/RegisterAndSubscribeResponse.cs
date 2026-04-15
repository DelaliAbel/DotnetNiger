namespace DotnetNiger.Community.Application.DTOs.Responses;

/// <summary>
/// Response for registration and newsletter subscription
/// </summary>
public class RegisterAndSubscribeResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool NewsletterSubscribed { get; set; }
    public string Message { get; set; } = string.Empty;
}
