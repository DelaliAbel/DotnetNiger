using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de recherche globale du site.</summary>
[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>Recherche du contenu selon une requête.</summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQueryRequest request)
    {
        var result = await searchService.SearchAsync(request);
        return Ok(new { Success = true, Data = result });
    }
}
