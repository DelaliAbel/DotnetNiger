namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un partenaire ou sponsor du DotnetNiger.
/// </summary>
public class Partner
{
    /// <summary>Identifiant unique du partenaire.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom du partenaire.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug unique pour l'URL.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description du partenaire.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du logo.</summary>
    public string LogoUrl { get; set; } = string.Empty;
    /// <summary>URL du site web du partenaire.</summary>
    public string WebsiteUrl { get; set; } = string.Empty;
    /// <summary>Type de partenaire (sponsor, membre, etc.).</summary>
    public string PartnerType { get; set; } = "sponsor";
    /// <summary>Ordre d'affichage.</summary>
    public int SortOrder { get; set; }
    /// <summary>Indique si le partenaire est actif.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Indique si le partenaire est supprimé logiquement.</summary>
    public bool IsDeleted { get; set; } = false;
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Date de dernière mise à jour.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
