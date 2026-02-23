using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les posts de la communauté
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Récupérer tous les posts avec pagination
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de posts par page (par défaut 10)</param>
    /// <returns>Liste paginée des posts</returns>
    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        try
        {
            var posts = await _postService.GetAllPublishedPostsAsync(page, pageSize);
            return Ok(new
            {
                page = page,
                pageSize = pageSize,
                total = posts.Count(),
                data = posts
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des posts", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un post par ID
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Détails du post</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostById(string id)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        try
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post non trouvé" });

            return Ok(post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du post", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer un nouveau post
    /// </summary>
    /// <param name="request">Données du post</param>
    /// <returns>Post créé</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Title))
            return BadRequest(new { message = "Titre requis" });

        try
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Slug = request.Title.ToLower().Replace(" ", "-"),
                AuthorId = Guid.NewGuid(),
                PostType = "Blog",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postService.CreatePostAsync(post);
            return CreatedAtAction(nameof(GetPostById), new { id = createdPost.Id }, createdPost);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du post", error = ex.Message });
        }
    }

    /// <summary>
    /// Mettre à jour un post
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <param name="request">Données à mettre à jour</param>
    /// <returns>Post mis à jour</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdatePostRequest request)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        try
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(new { message = "Post non trouvé" });

            if (!string.IsNullOrEmpty(request.Title))
                post.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Content))
                post.Content = request.Content;

            post.UpdatedAt = DateTime.UtcNow;
            var updatedPost = await _postService.UpdatePostAsync(post);
            return Ok(updatedPost);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du post", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un post
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        if (!Guid.TryParse(id, out var postId))
            return BadRequest(new { message = "ID du post invalide" });

        try
        {
            var deleted = await _postService.DeletePostAsync(postId);
            if (!deleted)
                return NotFound(new { message = "Post non trouvé" });

            return Ok(new { message = "Post supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du post", error = ex.Message });
        }
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
