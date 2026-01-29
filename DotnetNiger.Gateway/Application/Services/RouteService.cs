namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Détermine la route vers le service approprié
/// </summary>
public class RouteService : IRouteService
{
    private readonly ILogger<RouteService> _logger;

    public RouteService(ILogger<RouteService> logger)
    {
        _logger = logger;
    }

    public string DetermineServiceRoute(string path)
    {
        if (path.StartsWith("/identity", StringComparison.OrdinalIgnoreCase))
            return "identity";
        
        if (path.StartsWith("/community", StringComparison.OrdinalIgnoreCase))
            return "community";

        _logger.LogWarning("Route inconnue: {Path}", path);
        return string.Empty;
    }

    public bool IsValidRoute(string path)
    {
        return !string.IsNullOrEmpty(DetermineServiceRoute(path));
    }
}
