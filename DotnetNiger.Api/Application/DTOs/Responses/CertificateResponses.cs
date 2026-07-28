namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'information sur un certificat soumis.</summary>
public class CertificateResponse
{
    /// <summary>Identifiant du certificat.</summary>
    public Guid Id { get; set; }
    /// <summary>Statut du certificat (en attente, approuvé, rejeté).</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Date de soumission.</summary>
    public DateTime SubmissionDate { get; set; }
    /// <summary>Temps d'attente estimé pour la validation.</summary>
    public string EstimatedWaitTime { get; set; } = "24-48 heures";
    /// <summary>E-mail de support.</summary>
    public string SupportEmail { get; set; } = "support@dotnetniger.org";
}
