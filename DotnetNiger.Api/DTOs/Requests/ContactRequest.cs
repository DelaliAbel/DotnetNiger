namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de contact (formulaire de support).</summary>
public class ContactRequest
{
    /// <summary>Nom complet de l'expéditeur.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Adresse e-mail de l'expéditeur.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Objet du message.</summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>Corps du message.</summary>
    public string Message { get; set; } = string.Empty;
}
