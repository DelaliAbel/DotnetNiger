namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception lancée quand l'utilisateur n'a pas les permissions requises
/// </summary>
public class UnauthorizedAccessException : CommunityException
{
    public UnauthorizedAccessException(string message) 
        : base(message, 403)
    {
    }

    public UnauthorizedAccessException(string message, Exception innerException) 
        : base(message, 403, innerException)
    {
    }
}