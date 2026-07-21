using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers.General;

[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQueryRequest request)
    {
        var result = await searchService.SearchAsync(request);
        return Ok(new { Success = true, Data = result });
    }
}
