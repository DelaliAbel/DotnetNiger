using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Partenaire affiché sur le site.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Tier">Niveau de partenariat.</param>
public sealed record PartnerResponse(
    Guid Id,
    string Name,
    string Description,
    string WebsiteUrl,
    string? LogoUrl,
    PartnerTier Tier);

/// <summary>Demande de création d'un partenaire.</summary>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Tier">Niveau de partenariat.</param>
/// <param name="SortOrder">Ordre d'affichage.</param>
public sealed record CreatePartnerRequest(
    [Required, StringLength(150)] string Name,
    [Required, StringLength(500)] string Description,
    [Required, StringLength(2048)] string WebsiteUrl,
    PartnerTier Tier,
    [StringLength(2048)] string? LogoUrl = null,
    int SortOrder = 0);

/// <summary>Demande de mise à jour d'un partenaire.</summary>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo, ou <see langword="null"/> pour le retirer.</param>
/// <param name="Tier">Niveau de partenariat.</param>
/// <param name="SortOrder">Ordre d'affichage.</param>
public sealed record UpdatePartnerRequest(
    [Required, StringLength(150)] string Name,
    [Required, StringLength(500)] string Description,
    [Required, StringLength(2048)] string WebsiteUrl,
    PartnerTier Tier,
    [StringLength(2048)] string? LogoUrl = null,
    int SortOrder = 0);

