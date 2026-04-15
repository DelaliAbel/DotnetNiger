using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetNiger.Community.Api.Filters;

/// <summary>
/// Action filter pour gérer la mise en cache des réponses HTTP
/// Ajoute les headers de cache-control basés sur les attributs de contrôle du cache
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class CacheFilter : Attribute, IActionFilter
{
    private readonly int _durationInSeconds;
    private readonly bool _isPublic;

    /// <summary>
    /// Crée une nouvelle instance du filtre de cache
    /// </summary>
    /// <param name="durationInSeconds">Durée du cache en secondes (par défaut 300s = 5 minutes)</param>
    /// <param name="isPublic">Si true, le cache est public; sinon privé</param>
    public CacheFilter(int durationInSeconds = 300, bool isPublic = true)
    {
        _durationInSeconds = durationInSeconds;
        _isPublic = isPublic;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Nothing to do before action execution
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Response.StatusCode == StatusCodes.Status200OK ||
            context.HttpContext.Response.StatusCode == StatusCodes.Status304NotModified)
        {
            var cacheControl = _isPublic
                ? $"public, max-age={_durationInSeconds}"
                : $"private, max-age={_durationInSeconds}";

            context.HttpContext.Response.Headers.CacheControl = cacheControl;
        }
    }
}
