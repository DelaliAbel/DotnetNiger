using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = RoleConstants.AdminOrSuperAdmin)]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    [HttpGet("api/settings/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        var all = await settingsService.GetAllAsync();
        var dict = all.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

        return Ok(new
        {
            Success = true,
            Data = new
            {
                siteName = dict.GetValueOrDefault("site_name", ".NET Niger"),
                defaultOgImage = dict.GetValueOrDefault("default_og_image", "/images/og-default.jpg"),
                logoUrl = dict.GetValueOrDefault("logo_url", ""),
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await settingsService.GetAllAsync();
        return Ok(new { Success = true, Data = settings });
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await settingsService.GetByKeyAsync(key);
        if (setting is null)
            return NotFound(new { Success = false, Message = Messages.Setting.NotFound });
        return Ok(new { Success = true, Data = setting });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSiteSettingRequest request)
    {
        var setting = await settingsService.SetAsync(key, request.Value);
        return Ok(new { Success = true, Data = setting, Message = Messages.Setting.Updated });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBatch([FromBody] UpdateSiteSettingsRequest request)
    {
        await settingsService.SetBatchAsync(request.Settings);
        return Ok(new { Success = true, Message = Messages.Setting.BatchUpdated });
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var deleted = await settingsService.DeleteAsync(key);
        if (!deleted)
            return NotFound(new { Success = false, Message = Messages.Setting.NotFound });
        return Ok(new { Success = true, Message = Messages.Setting.Deleted });
    }
}
