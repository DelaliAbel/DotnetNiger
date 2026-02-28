namespace DotnetNiger.Gateway.Application.Exceptions;

/// <summary>
/// Exception levée quand la limite de taux est dépassée.
/// </summary>
public sealed class RateLimitExceededException : GatewayException
{
    public string ClientId { get; }
    public string Endpoint { get; }

    public RateLimitExceededException(string clientId, string endpoint)
        : base($"Rate limit exceeded for client '{clientId}' on endpoint '{endpoint}'", 429)
    {
        ClientId = clientId;
        Endpoint = endpoint;
    }
}
