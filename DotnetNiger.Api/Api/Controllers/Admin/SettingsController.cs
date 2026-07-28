using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.Admin;

/// <summary>Contrôleur de gestion des paramètres du site.</summary>
[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = "admin.settings.manage")]
public class SettingsController(ISettingsService settingsService) : BaseController
{
    /// <summary>Récupère les paramètres publics du site.</summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        var settings = await settingsService.GetPublicSettingsAsync();
        return Success(settings);
    }

    /// <summary>Récupère tous les paramètres du site.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await settingsService.GetAllAsync();
        return Success(settings);
    }

    /// <summary>Récupère un paramètre par sa clé.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await settingsService.GetByKeyAsync(key);
        if (setting is null)
            return NotFound(Messages.Setting.NotFound);
        return Success(setting);
    }

    /// <summary>Met à jour un paramètre par sa clé.</summary>
    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSiteSettingRequest request)
    {
        var setting = await settingsService.SetAsync(key, request.Value);
        return Success(setting, Messages.Setting.Updated);
    }

    /// <summary>Met à jour plusieurs paramètres en une seule requête.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateBatch([FromBody] UpdateSiteSettingsRequest request)
    {
        await settingsService.SetBatchAsync(request.Settings);
        return Success<object?>(null, Messages.Setting.BatchUpdated);
    }

    /// <summary>Supprime un paramètre par sa clé.</summary>
    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var deleted = await settingsService.DeleteAsync(key);
        if (!deleted)
            return NotFound(Messages.Setting.NotFound);
        return Success<object?>(null, Messages.Setting.Deleted);
    }
}
