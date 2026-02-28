namespace DotnetNiger.Gateway.Application.Exceptions;

/// <summary>
/// Exception levée quand un service backend n'est pas disponible.
/// </summary>
public sealed class ServiceUnavailableException : GatewayException
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName)
        : base($"Service '{serviceName}' is currently unavailable", 503)
    {
        ServiceName = serviceName;
    }

    public ServiceUnavailableException(string serviceName, Exception innerException)
        : base($"Service '{serviceName}' is currently unavailable", innerException, 503)
    {
        ServiceName = serviceName;
    }
}
