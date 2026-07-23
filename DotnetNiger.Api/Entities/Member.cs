namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un membre du DotnetNiger avec son profil public.
/// </summary>
public class Member
{
    /// <summary>Identifiant unique du membre.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur associé.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom d'affichage du membre.</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>Adresse email du membre.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Nom complet du membre.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Rôles du membre (séparés par des virgules).</summary>
    public string Roles { get; set; } = string.Empty;
    /// <summary>Biographie du membre.</summary>
    public string Bio { get; set; } = string.Empty;
    /// <summary>URL de l'avatar du membre.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Numéro de téléphone du membre.</summary>
    public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>Pays du membre.</summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>Ville du membre.</summary>
    public string City { get; set; } = string.Empty;
    /// <summary>Localisation détaillée.</summary>
    public string? Location { get; set; }
    /// <summary>URL du site web personnel.</summary>
    public string? WebsiteUrl { get; set; }
    /// <summary>Indique si le membre fait partie de l'équipe.</summary>
    public bool IsTeamMember { get; set; }
    /// <summary>Poste ou titre du membre.</summary>
    public string Position { get; set; } = string.Empty;
    /// <summary>Date de création du profil.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour du profil.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation vers l'utilisateur.</summary>
    public ApplicationUser? User { get; set; }
    /// <summary>Liens réseaux sociaux du membre.</summary>
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
    /// <summary>Certificats du membre.</summary>
    public ICollection<Certificate> Certificates { get; set; } = [];
    /// <summary>Compétences du membre.</summary>
    public ICollection<MemberSkill> Skills { get; set; } = [];
}
