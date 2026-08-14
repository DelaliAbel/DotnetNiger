using System.Threading;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

[ApiController]
[Route("api/settings")]
public class PublicSettingsController(ISettingsService settingsService) : BaseController
{
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken ct = default)
    {
        var settings = await settingsService.GetPublicSettingsAsync(ct);
        return Success(settings);
    }
}
