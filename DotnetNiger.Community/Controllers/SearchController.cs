using DotnetNiger.Community.Dtos.Requests;
using DotnetNiger.Community.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQueryRequest request)
    {
        var result = await searchService.SearchAsync(request);
        return Ok(new { Success = true, Data = result });
    }
}
