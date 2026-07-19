namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de formulaire de contact.</summary>
public class ContactRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
