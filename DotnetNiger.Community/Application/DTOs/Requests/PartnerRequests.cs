using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de création d'un partenaire.</summary>
public class CreatePartnerRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string PartnerType { get; set; } = "sponsor";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Requête de mise à jour d'un partenaire.</summary>
public class UpdatePartnerRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string PartnerType { get; set; } = "sponsor";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
