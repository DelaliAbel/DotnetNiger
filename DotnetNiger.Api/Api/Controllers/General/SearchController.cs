using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

/// <summary>Contrôleur de recherche globale du site.</summary>
[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : BaseController
{
    /// <summary>Recherche du contenu selon une requête.</summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQueryRequest request)
    {
        var result = await searchService.SearchAsync(request);
        return Success(result);
    }
}
