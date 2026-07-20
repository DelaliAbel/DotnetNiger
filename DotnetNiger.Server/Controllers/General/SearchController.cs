using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Server.Controllers.General;

[ApiController]
[Route("api/[controller]")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQueryRequest request)
    {
        var result = await searchService.SearchAsync(request);
        return Ok(new { Success = true, Data = result });
    }
}
