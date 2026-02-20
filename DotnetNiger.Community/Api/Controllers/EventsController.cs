using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les événements
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les événements
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre d'événements par page (par défaut 10)</param>
    /// <returns>Liste paginée des événements</returns>
    [HttpGet]
    public IActionResult GetEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        return Ok(new
        {
            page = page,
            pageSize = pageSize,
            total = 0,
            data = new List<object>()
        });
    }

    /// <summary>
    /// Récupérer un événement par ID
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <returns>Détails de l'événement</returns>
    [HttpGet("{id}")]
    public IActionResult GetEventById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID de l'événement requis" });

        return NotFound(new { message = "Événement non trouvé" });
    }

    /// <summary>
    /// Créer un nouvel événement
    /// </summary>
    /// <param name="request">Données de l'événement</param>
    /// <returns>Événement créé</returns>
    [HttpPost]
    public IActionResult CreateEvent([FromBody] CreateEventRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequest(new { message = "Titre requis" });

        return CreatedAtAction(nameof(GetEventById), new { id = "new-id" }, new { id = "new-id" });
    }

    /// <summary>
    /// Mettre à jour un événement
    /// </summary>
    /// <param name="id">ID de l'événement</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Événement mis à jour</returns>
    [HttpPut("{id}")]
    public IActionResult UpdateEvent(string id, [FromBody] UpdateEventRequest request)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID de l'événement requis" });

        return Ok(new { message = "Événement mis à jour" });
    }
}

/// <summary>
/// DTO pour créer un événement
/// </summary>
public class CreateEventRequest
{
    /// <summary>Titre de l'événement</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Description de l'événement</summary>
    public string? Description { get; set; }
    /// <summary>Date de début</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Date de fin</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>Localisation</summary>
    public string? Location { get; set; }
    /// <summary>Tags de l'événement</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un événement
/// </summary>
public class UpdateEventRequest
{
    /// <summary>Titre de l'événement</summary>
    public string? Title { get; set; }
    /// <summary>Description de l'événement</summary>
    public string? Description { get; set; }
    /// <summary>Date de début</summary>
    public DateTime? StartDate { get; set; }
}
