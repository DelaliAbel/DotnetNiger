using Asp.Versioning;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.Constants;
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>Gestion des paramètres globaux du site (réservé SuperAdmin).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/settings")]
[Authorize(Roles = RoleConstants.SuperAdmin)]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    /// <summary>Retourne tous les paramètres du site.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await settingsService.GetAllAsync();
        return Ok(new { Success = true, Data = settings });
    }

    /// <summary>Recherche un paramètre par sa clé.</summary>
    /// <param name="key">Clé du paramètre.</param>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await settingsService.GetByKeyAsync(key);
        if (setting is null)
            return NotFound(new { Success = false, Message = Messages.Setting.NotFound });
        return Ok(new { Success = true, Data = setting });
    }

    /// <summary>Met à jour un paramètre existant ou le crée.</summary>
    /// <param name="key">Clé du paramètre.</param>
    /// <param name="request">Nouvelle valeur.</param>
    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSiteSettingRequest request)
    {
        var setting = await settingsService.SetAsync(key, request.Value);
        return Ok(new { Success = true, Data = setting, Message = Messages.Setting.Updated });
    }

    /// <summary>Met à jour plusieurs paramètres en une seule requête.</summary>
    /// <param name="request">Dictionnaire clé/valeur des paramètres.</param>
    [HttpPut]
    public async Task<IActionResult> UpdateBatch([FromBody] UpdateSiteSettingsRequest request)
    {
        await settingsService.SetBatchAsync(request.Settings);
        return Ok(new { Success = true, Message = Messages.Setting.BatchUpdated });
    }

    /// <summary>Supprime un paramètre par sa clé.</summary>
    /// <param name="key">Clé du paramètre à supprimer.</param>
    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var deleted = await settingsService.DeleteAsync(key);
        if (!deleted)
            return NotFound(new { Success = false, Message = Messages.Setting.NotFound });
        return Ok(new { Success = true, Message = Messages.Setting.Deleted });
    }
}
