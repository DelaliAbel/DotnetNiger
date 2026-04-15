using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services.Interfaces;
using Microsoft.AspNetCore.OutputCaching;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour gérer les tags
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tags")]
public class TagsController : ApiControllerBase
{
    private readonly ITagService _tagService;
    private readonly ICommunityRequestMapper _requestMapper;

    public TagsController(ITagService tagService, ICommunityRequestMapper requestMapper)
    {
        _tagService = tagService;
        _requestMapper = requestMapper;
    }

    /// <summary>
    /// Récupérer tous les tags
    /// </summary>
    /// <returns>Liste des tags</returns>
    [HttpGet]
    [OutputCache(PolicyName = "HotReadPolicy")]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Success(tags);
    }

    /// <summary>
    /// Récupérer un tag par ID
    /// </summary>
    /// <param name="id">ID du tag</param>
    /// <returns>Détails du tag</returns>
    [HttpGet("{id}")]
    [OutputCache(PolicyName = "HotReadPolicy")]
    public async Task<IActionResult> GetTagById(string id)
    {
        var tagId = ParseGuidOrThrow(id, nameof(id), "ID du tag invalide");

        var tag = await _tagService.GetTagByIdAsync(tagId);
        if (tag == null)
            return NotFoundProblem("Tag non trouve");

        return Success(tag);
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
            return BadRequestProblem("Nom du tag requis");

        var tag = _requestMapper.MapToTag(request);
        var createdTag = await _tagService.CreateTagAsync(tag);
        return CreatedSuccess(nameof(GetTagById), new { id = createdTag.Id }, createdTag, "Tag cree avec succes");
    }

    /// <summary>
    /// Supprimer un tag
    /// </summary>
    /// <param name="id">ID du tag</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(string id)
    {
        var tagId = ParseGuidOrThrow(id, nameof(id), "ID du tag invalide");

        var deleted = await _tagService.DeleteTagAsync(tagId);
        if (!deleted)
            return NotFoundProblem("Tag non trouve");

        return SuccessMessage("Tag supprime avec succes");
    }
}

