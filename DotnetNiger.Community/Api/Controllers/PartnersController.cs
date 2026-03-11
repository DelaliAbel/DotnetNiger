using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les partenaires
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    /// <summary>
    /// Récupérer tous les partenaires
    /// </summary>
    /// <returns>Liste des partenaires</returns>
    [HttpGet]
    public async Task<IActionResult> GetPartners()
    {
        try
        {
            var partners = await _partnerService.GetAllPartnersAsync();
            return Ok(new { data = partners });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des partenaires", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un partenaire par ID
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <returns>Détails du partenaire</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPartnerById(string id)
    {
        if (!Guid.TryParse(id, out var partnerId))
            return BadRequest(new { message = "ID du partenaire invalide" });

        try
        {
            var partner = await _partnerService.GetPartnerByIdAsync(partnerId);
            if (partner == null)
                return NotFound(new { message = "Partenaire non trouvé" });

            return Ok(partner);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du partenaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer un nouveau partenaire
    /// </summary>
    /// <param name="request">Données du partenaire</param>
    /// <returns>Partenaire créé</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du partenaire requis" });

        try
        {
            var partner = new Partner
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-"),
                LogoUrl = request.LogoUrl ?? string.Empty,
                Website = request.Website ?? string.Empty,
                Description = request.Description ?? string.Empty,
                PartnerType = request.PartnerType ?? "Silver",
                Level = request.Level ?? "Gold",
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

            var createdPartner = await _partnerService.CreatePartnerAsync(partner);
            return CreatedAtAction(nameof(GetPartnerById), new { id = createdPartner.Id }, createdPartner);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du partenaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour un partenaire
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Partenaire mis à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePartner(string id, [FromBody] UpdatePartnerRequest request)
    {
        if (!Guid.TryParse(id, out var partnerId))
            return BadRequest(new { message = "ID du partenaire invalide" });

        try
        {
            var partner = await _partnerService.GetPartnerByIdAsync(partnerId);
            if (partner == null)
                return NotFound(new { message = "Partenaire non trouvé" });

            if (!string.IsNullOrEmpty(request.Name))
                partner.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                partner.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Website))
                partner.Website = request.Website;

            var updatedPartner = await _partnerService.UpdatePartnerAsync(partner);
            return Ok(updatedPartner);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du partenaire", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un partenaire
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePartner(string id)
    {
        if (!Guid.TryParse(id, out var partnerId))
            return BadRequest(new { message = "ID du partenaire invalide" });

        try
        {
            var deleted = await _partnerService.DeletePartnerAsync(partnerId);
            if (!deleted)
                return NotFound(new { message = "Partenaire non trouvé" });

            return Ok(new { message = "Partenaire supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du partenaire", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO pour créer un partenaire
/// </summary>
public class CreatePartnerRequest
{
    /// <summary>Nom du partenaire</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Description du partenaire</summary>
    public string? Description { get; set; }
    /// <summary>URL du site web</summary>
    public string? Website { get; set; }
    /// <summary>URL du logo</summary>
    public string? LogoUrl { get; set; }
    /// <summary>Type de partenariat</summary>
    public string? PartnerType { get; set; }
    /// <summary>Niveau de partenariat</summary>
    public string? Level { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un partenaire
/// </summary>
public class UpdatePartnerRequest
{
    /// <summary>Nom du partenaire</summary>
    public string? Name { get; set; }
    /// <summary>Description du partenaire</summary>
    public string? Description { get; set; }
    /// <summary>URL du site web</summary>
    public string? Website { get; set; }
}
