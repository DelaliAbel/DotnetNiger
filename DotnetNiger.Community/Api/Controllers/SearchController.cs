using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

/// <summary>
/// Controller pour la recherche dans le service Community
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    /// <summary>
    /// Rechercher du contenu
    /// </summary>
    /// <param name="query">Terme de recherche</param>
    /// <returns>Résultats de la recherche</returns>
    [HttpGet]
    public IActionResult Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Terme de recherche requis" });

        return Ok(new
        {
            query = query,
            results = new
            {
                posts = new List<object>(),
                events = new List<object>(),
                projects = new List<object>(),
                resources = new List<object>()
            }
        });
    }
}
