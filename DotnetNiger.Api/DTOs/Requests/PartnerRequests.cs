using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.DTOs.Requests;

/// <summary>Requête de création d'un partenaire.</summary>
public class CreatePartnerRequest
{
    /// <summary>Nom du partenaire (max 200 caractères).</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Description du partenaire.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du logo.</summary>
    public string LogoUrl { get; set; } = string.Empty;
    /// <summary>URL du site web du partenaire.</summary>
    public string WebsiteUrl { get; set; } = string.Empty;
    /// <summary>Type de partenaire (sponsor, partenaire technique, etc.).</summary>
    public string PartnerType { get; set; } = "sponsor";
    /// <summary>Ordre d'affichage.</summary>
    public int SortOrder { get; set; }
    /// <summary>Indique si le partenaire est actif.</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>Requête de mise à jour d'un partenaire.</summary>
public class UpdatePartnerRequest
{
    /// <summary>Nom du partenaire (max 200 caractères).</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Description du partenaire.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du logo.</summary>
    public string LogoUrl { get; set; } = string.Empty;
    /// <summary>URL du site web du partenaire.</summary>
    public string WebsiteUrl { get; set; } = string.Empty;
    /// <summary>Type de partenaire.</summary>
    public string PartnerType { get; set; } = "sponsor";
    /// <summary>Ordre d'affichage.</summary>
    public int SortOrder { get; set; }
    /// <summary>Indique si le partenaire est actif.</summary>
    public bool IsActive { get; set; } = true;
}
