using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les commentaires
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les commentaires
    /// </summary>
    /// <returns>Liste des commentaires</returns>
    [HttpGet]
    public IActionResult GetComments([FromQuery] int pageSize = 10)
    {
        return Ok(new { data = new List<object>() });
    }

    /// <summary>
    /// Récupérer un commentaire par ID
    /// </summary>
    /// <param name="id">ID du commentaire</param>
    /// <returns>Détails du commentaire</returns>
    [HttpGet("{id}")]
    public IActionResult GetCommentById(string id)
    {
        return NotFound(new { message = "Commentaire non trouvé" });
    }

    /// <summary>
    /// Créer un nouveau commentaire
    /// </summary>
    /// <param name="request">Données du commentaire</param>
    /// <returns>Commentaire créé</returns>
    [HttpPost]
    public IActionResult CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Content))
            return BadRequest(new { message = "Contenu requis" });

        return CreatedAtAction(nameof(GetCommentById), new { id = "new-id" }, new { id = "new-id" });
    }
}

/// <summary>
/// DTO pour créer un commentaire
/// </summary>
public class CreateCommentRequest
{
    /// <summary>Contenu du commentaire</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>ID du post concerné</summary>
    public string? PostId { get; set; }
}
