namespace DotnetNiger.Gateway.Application.Exceptions;

/// <summary>
/// Exception de base pour le Gateway
/// </summary>
public class GatewayException : Exception
{
    public int StatusCode { get; set; }

    public GatewayException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public GatewayException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
