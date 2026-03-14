using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les posts de la communauté
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController : ApiControllerBase
{
    private readonly IPostService _postService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunityRequestMapper _requestMapper;

    public PostsController(
        IPostService postService,
        ICurrentUserService currentUserService,
        ICommunityRequestMapper requestMapper)
    {
        _postService = postService;
        _currentUserService = currentUserService;
        _requestMapper = requestMapper;
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
            return BadRequestProblem("Parametres de pagination invalides");

        var posts = await _postService.GetAllPublishedPostsAsync(page, pageSize);
        return Success(posts, meta: new { page, pageSize, total = posts.Count() });
    }

    /// <summary>
    /// Récupérer un post par ID
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Détails du post</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostById(string id)
    {
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null)
            return NotFoundProblem("Post non trouve");

        return Success(post);
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
            return BadRequestProblem("Titre requis");

        var currentUserId = _currentUserService.GetRequiredUserId();
        var post = _requestMapper.MapToPost(request, currentUserId);

        var createdPost = await _postService.CreatePostAsync(post);
        return CreatedSuccess(nameof(GetPostById), new { id = createdPost.Id }, createdPost, "Post cree avec succes");
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
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null)
            return NotFoundProblem("Post non trouve");

        _requestMapper.ApplyPostUpdates(post, request);
        var updatedPost = await _postService.UpdatePostAsync(post);
        return Success(updatedPost, "Post mis a jour avec succes");
    }

    /// <summary>
    /// Supprimer un post
    /// </summary>
    /// <param name="id">ID du post</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(string id)
    {
        var postId = ParseGuidOrThrow(id, nameof(id), "ID du post invalide");

        var deleted = await _postService.DeletePostAsync(postId);
        if (!deleted)
            return NotFoundProblem("Post non trouve");

        return SuccessMessage("Post supprime avec succes");
    }
}
