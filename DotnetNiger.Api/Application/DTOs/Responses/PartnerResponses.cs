namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Réponse d'un partenaire.</summary>
public class PartnerResponse
{
    /// <summary>Identifiant du partenaire.</summary>
    public Guid Id { get; set; }
    /// <summary>Nom du partenaire.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Slug URL du partenaire.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Description du partenaire.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>URL du logo.</summary>
    public string LogoUrl { get; set; } = string.Empty;
    /// <summary>URL du site web.</summary>
    public string WebsiteUrl { get; set; } = string.Empty;
    /// <summary>Type de partenaire.</summary>
    public string PartnerType { get; set; } = string.Empty;
    /// <summary>Ordre d'affichage.</summary>
    public int SortOrder { get; set; }
    /// <summary>Indique si le partenaire est actif.</summary>
    public bool IsActive { get; set; }
    /// <summary>Date de création.</summary>
    public DateTime CreatedAt { get; set; }
}
