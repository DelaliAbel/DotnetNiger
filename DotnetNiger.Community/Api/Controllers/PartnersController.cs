using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les partenaires
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les partenaires
    /// </summary>
    /// <returns>Liste des partenaires</returns>
    [HttpGet]
    public IActionResult GetPartners()
    {
        return Ok(new { data = new List<object>() });
    }

    /// <summary>
    /// Récupérer un partenaire par ID
    /// </summary>
    /// <param name="id">ID du partenaire</param>
    /// <returns>Détails du partenaire</returns>
    [HttpGet("{id}")]
    public IActionResult GetPartnerById(string id)
    {
        return NotFound(new { message = "Partenaire non trouvé" });
    }

    /// <summary>
    /// Créer un nouveau partenaire
    /// </summary>
    /// <param name="request">Données du partenaire</param>
    /// <returns>Partenaire créé</returns>
    [HttpPost]
    public IActionResult CreatePartner([FromBody] CreatePartnerRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du partenaire requis" });

        return CreatedAtAction(nameof(GetPartnerById), new { id = "new-id" }, new { id = "new-id" });
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
}
