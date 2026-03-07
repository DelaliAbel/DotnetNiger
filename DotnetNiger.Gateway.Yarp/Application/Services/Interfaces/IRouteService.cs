namespace DotnetNiger.Gateway.Application.Services.Interfaces;

/// <summary>
/// Interface pour le service de routage
/// </summary>
public interface IRouteService
{
    string DetermineServiceRoute(string path);
    bool IsValidRoute(string path);
}
