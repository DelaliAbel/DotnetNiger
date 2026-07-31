namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'information sur un certificat soumis.</summary>
public class CertificateResponse
{
    /// <summary>Identifiant du certificat.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur ayant soumis le certificat.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom de la personne ayant soumis le certificat.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>Email de la personne ayant soumis le certificat.</summary>
    public string UserEmail { get; set; } = string.Empty;
    /// <summary>URL de l'avatar de la personne ayant soumis le certificat.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>URL du certificat soumis.</summary>
    public string CertificateUrl { get; set; } = string.Empty;
    /// <summary>Type de certificat (AWS, Azure, etc.).</summary>
    public string CertificateType { get; set; } = string.Empty;
    /// <summary>Statut du certificat (en attente, approuvé, rejeté).</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Date de soumission.</summary>
    public DateTime SubmissionDate { get; set; }
    /// <summary>Notes de la revue.</summary>
    public string? ReviewedNotes { get; set; }
    /// <summary>Date de la revue.</summary>
    public DateTime? ReviewedAt { get; set; }
    /// <summary>Temps d'attente estimé pour la validation.</summary>
    public string EstimatedWaitTime { get; set; } = "24-48 heures";
    /// <summary>E-mail de support.</summary>
    public string SupportEmail { get; set; } = "support@dotnetniger.org";
}
