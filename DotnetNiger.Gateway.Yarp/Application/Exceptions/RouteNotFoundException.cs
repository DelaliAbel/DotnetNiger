namespace DotnetNiger.Gateway.Application.Exceptions;

/// <summary>
/// Exception levée quand une route n'est pas trouvée
/// </summary>
public class RouteNotFoundException : GatewayException
{
    public RouteNotFoundException(string path)
        : base($"Route not found: {path}", 404)
    {
    }
}
