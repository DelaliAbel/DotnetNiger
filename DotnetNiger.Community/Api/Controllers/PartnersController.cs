using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les partenaires
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PartnersController : ApiControllerBase
{
    private readonly IPartnerService _partnerService;
    private readonly ICommunityRequestMapper _requestMapper;

    public PartnersController(IPartnerService partnerService, ICommunityRequestMapper requestMapper)
    {
        _partnerService = partnerService;
        _requestMapper = requestMapper;
    }

    /// <summary>
    /// Récupérer tous les partenaires
    /// </summary>
    /// <returns>Liste des partenaires</returns>
    [HttpGet]
    public async Task<IActionResult> GetPartners()
    {
        var partners = await _partnerService.GetAllPartnersAsync();
        return Success(partners);
    }

    /// <summary>
    /// Récupérer un partenaire par ID
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <returns>Détails du partenaire</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPartnerById(string id)
    {
        var partnerId = ParseGuidOrThrow(id, nameof(id), "ID du partenaire invalide");

        var partner = await _partnerService.GetPartnerByIdAsync(partnerId);
        if (partner == null)
            return NotFoundProblem("Partenaire non trouve");

        return Success(partner);
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
            return BadRequestProblem("Nom du partenaire requis");

        var partner = _requestMapper.MapToPartner(request);
        var createdPartner = await _partnerService.CreatePartnerAsync(partner);
        return CreatedSuccess(nameof(GetPartnerById), new { id = createdPartner.Id }, createdPartner, "Partenaire cree avec succes");
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
        var partnerId = ParseGuidOrThrow(id, nameof(id), "ID du partenaire invalide");

        var partner = await _partnerService.GetPartnerByIdAsync(partnerId);
        if (partner == null)
            return NotFoundProblem("Partenaire non trouve");

        _requestMapper.ApplyPartnerUpdates(partner, request);
        var updatedPartner = await _partnerService.UpdatePartnerAsync(partner);
        return Success(updatedPartner, "Partenaire mis a jour avec succes");
    }

    /// <summary>
    /// Supprimer un partenaire
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePartner(string id)
    {
        var partnerId = ParseGuidOrThrow(id, nameof(id), "ID du partenaire invalide");

        var deleted = await _partnerService.DeletePartnerAsync(partnerId);
        if (!deleted)
            return NotFoundProblem("Partenaire non trouve");

        return SuccessMessage("Partenaire supprime avec succes");
    }
}

