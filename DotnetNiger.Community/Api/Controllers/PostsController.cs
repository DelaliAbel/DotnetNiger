using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les posts de la communauté
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    /// <summary>
    /// Récupérer tous les posts avec pagination
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de posts par page (par défaut 10)</param>
    /// <returns>Liste paginée des posts</returns>
    [HttpGet]
    public IActionResult GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
    /// Récupérer un post par ID
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Détails du post</returns>
    [HttpGet("{id}")]
    public IActionResult GetPostById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID du post requis" });

        return NotFound(new { message = "Post non trouvé" });
    }

    /// <summary>
    /// Créer un nouveau post
    /// </summary>
    /// <param name="request">Données du post</param>
    /// <returns>Post créé</returns>
    [HttpPost]
    public IActionResult CreatePost([FromBody] CreatePostRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequest(new { message = "Titre requis" });

        return CreatedAtAction(nameof(GetPostById), new { id = "new-id" }, new { id = "new-id" });
    }

    /// <summary>
    /// Mettre à jour un post
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Post mis à jour</returns>
    [HttpPut("{id}")]
    public IActionResult UpdatePost(string id, [FromBody] UpdatePostRequest request)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID du post requis" });

        return Ok(new { message = "Post mis à jour" });
    }

    /// <summary>
    /// Supprimer un post
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public IActionResult DeletePost(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "ID du post requis" });

        return Ok(new { message = "Post supprimé" });
    }
}

/// <summary>
/// DTO pour créer un post
/// </summary>
public class CreatePostRequest
{
    /// <summary>Titre du post</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Contenu du post</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Tags du post</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un post
/// </summary>
public class UpdatePostRequest
{
    /// <summary>Titre du post</summary>
    public string? Title { get; set; }
    /// <summary>Contenu du post</summary>
    public string? Content { get; set; }
    /// <summary>Tags du post</summary>
    public List<string>? Tags { get; set; }
}
