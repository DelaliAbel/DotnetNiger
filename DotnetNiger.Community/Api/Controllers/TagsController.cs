using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Récupérer tous les tags
    /// </summary>
    /// <returns>Liste des tags</returns>
    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        try
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(new { data = tags });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération des tags", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupérer un tag par ID
    /// </summary>
    /// <param name="id">ID du tag</param>
    /// <returns>Détails du tag</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTagById(string id)
    {
        if (!Guid.TryParse(id, out var tagId))
            return BadRequest(new { message = "ID du tag invalide" });

        try
        {
            var tag = await _tagService.GetTagByIdAsync(tagId);
            if (tag == null)
                return NotFound(new { message = "Tag non trouvé" });

            return Ok(tag);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du tag", error = ex.Message });
        }
    }

    /// <summary>
    /// Créer un nouveau tag
    /// </summary>
    /// <param name="request">Données du tag</param>
    /// <returns>Tag créé</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Nom du tag requis" });

        try
        {
            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = request.Name.ToLower().Replace(" ", "-"),
                PostCount = 0
            };

            var createdTag = await _tagService.CreateTagAsync(tag);
            return CreatedAtAction(nameof(GetTagById), new { id = createdTag.Id }, createdTag);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la création du tag", error = ex.Message });
        }
    }

    /// <summary>
    /// Supprimer un tag
    /// </summary>
    /// <param name="id">ID du tag</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(string id)
    {
        if (!Guid.TryParse(id, out var tagId))
            return BadRequest(new { message = "ID du tag invalide" });

        try
        {
            var deleted = await _tagService.DeleteTagAsync(tagId);
            if (!deleted)
                return NotFound(new { message = "Tag non trouvé" });

            return Ok(new { message = "Tag supprimé avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la suppression du tag", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO pour créer un tag
/// </summary>
public class CreateTagRequest
{
    /// <summary>Nom du tag</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour mettre à jour un tag
/// </summary>
public class UpdateTagRequest
{
    /// <summary>Nom du tag</summary>
    public string? Name { get; set; }
}
