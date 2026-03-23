namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception de base pour tous les erreurs du service Community
/// </summary>
public class CommunityException : Exception
{
    public int StatusCode { get; set; }

    public CommunityException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public CommunityException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
