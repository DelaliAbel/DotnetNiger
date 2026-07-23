namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un certificat soumis par un membre.
/// </summary>
public class Certificate
{
    /// <summary>Identifiant unique du certificat.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur propriétaire.</summary>
    public Guid UserId { get; set; }
    /// <summary>Identifiant du membre.</summary>
    public Guid MemberId { get; set; }
    /// <summary>Nom du certificat.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Organisme émetteur du certificat.</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>URL du certificat (image ou PDF).</summary>
    public string CertificateUrl { get; set; } = string.Empty;
    /// <summary>Type de certificat.</summary>
    public string CertificateType { get; set; } = string.Empty;
    /// <summary>Statut de validation du certificat.</summary>
    public string Status { get; set; } = "Pending";
    /// <summary>Date de soumission.</summary>
    public DateTime SubmissionDate { get; set; }
    /// <summary>Notes de la revue.</summary>
    public string? ReviewedNotes { get; set; }
    /// <summary>Date de la revue.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
    /// <summary>Navigation vers le membre.</summary>
    public Member Member { get; set; } = null!;
}
