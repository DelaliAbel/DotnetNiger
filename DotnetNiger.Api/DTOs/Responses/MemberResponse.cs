namespace DotnetNiger.Api.DTOs.Responses;

/// <summary>Réponse d'un membre de la communauté.</summary>
public class MemberResponse
{
    /// <summary>Identifiant du membre.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant de l'utilisateur associé.</summary>
    public Guid UserId { get; set; }
    /// <summary>Nom d'affichage du membre.</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>Nom complet du membre.</summary>
    public string FullName { get; set; } = string.Empty;
    /// <summary>Biographie du membre.</summary>
    public string? Bio { get; set; }
    /// <summary>URL de l'avatar du membre.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Poste ou titre du membre.</summary>
    public string Position { get; set; } = string.Empty;
    /// <summary>Localisation du membre.</summary>
    public string? Location { get; set; }
    /// <summary>URL du site web du membre.</summary>
    public string? WebsiteUrl { get; set; }
    /// <summary>Liens réseaux sociaux du membre.</summary>
    public List<SocialLinkResponse> SocialLinks { get; set; } = [];
    /// <summary>Date de création du profil.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime? UpdatedAt { get; set; }
}
