namespace DotnetNiger.Gateway.Application.Services;

/// <summary>
/// Interface pour le service de forwarding de requêtes
/// </summary>
public interface IRequestForwardingService
{
    Task<HttpResponseMessage> ForwardRequestAsync(string serviceUrl, HttpRequest request);
}
