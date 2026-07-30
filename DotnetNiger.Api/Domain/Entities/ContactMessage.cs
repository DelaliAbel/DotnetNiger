namespace DotnetNiger.Api.Domain.Entities;

/// <summary>
/// Représente un message envoyé via le formulaire de contact.
/// </summary>
public class ContactMessage
{
    /// <summary>Identifiant unique du message.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom complet de l'expéditeur.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Adresse email de l'expéditeur.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Objet du message.</summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>Contenu du message.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Date de réception du message.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Indique si le message a été lu.</summary>
    public bool IsRead { get; set; }
}
