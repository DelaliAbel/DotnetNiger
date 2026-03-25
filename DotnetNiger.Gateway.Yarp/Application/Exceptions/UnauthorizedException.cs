namespace DotnetNiger.Gateway.Application.Exceptions;

/// <summary>
/// Exception levée quand l'utilisateur n'est pas autorisé.
/// </summary>
public sealed class UnauthorizedException : GatewayException
{
    public UnauthorizedException()
        : base("Unauthorized access", 401)
    {
    }

    public UnauthorizedException(string message)
        : base(message, 401)
    {
    }
}
