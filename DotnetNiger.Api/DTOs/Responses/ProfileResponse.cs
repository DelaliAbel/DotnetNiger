namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse complète du profil utilisateur.</summary>
public class ProfileResponse
{
    /// <summary>Identifiant de l'utilisateur.</summary>
    public Guid Id { get; set; }
    /// <summary>Adresse e-mail.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Nom d'utilisateur.</summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>Prénom.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Nom de famille.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Nom complet.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Biographie.</summary>
    public string Bio { get; set; } = string.Empty;
    /// <summary>URL de l'avatar.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Numéro de téléphone.</summary>
    public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>Pays de résidence.</summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>Ville de résidence.</summary>
    public string City { get; set; } = string.Empty;
    /// <summary>Indique si le compte est actif.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Indique si c'est un membre de l'équipe.</summary>
    public bool IsTeamMember { get; set; }
    /// <summary>Poste occupé.</summary>
    public string Position { get; set; } = string.Empty;
    /// <summary>Date de création du compte.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Date de dernière connexion.</summary>
    public DateTime? LastLoginAt { get; set; }
    /// <summary>Compétences techniques.</summary>
    public List<string> Skills { get; set; } = [];
    /// <summary>Rôles attribués.</summary>
    public List<string> Roles { get; set; } = [];
    /// <summary>Liens sociaux du profil.</summary>
    public List<SocialLinkResponse> SocialLinks { get; set; } = [];
    /// <summary>Informations du certificat (si soumis).</summary>
    public CertificateInfo? Certificate { get; set; }
}

/// <summary>Informations d'un certificat soumis par l'utilisateur.</summary>
public class CertificateInfo
{
    /// <summary>Statut du certificat.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Type de certificat.</summary>
    public string CertificateType { get; set; } = string.Empty;
    /// <summary>Date de soumission.</summary>
    public DateTime SubmissionDate { get; set; }
    /// <summary>Notes de révision éventuelles.</summary>
    public string? ReviewedNotes { get; set; }
    /// <summary>Date de révision.</summary>
    public DateTime? ReviewedAt { get; set; }
}
