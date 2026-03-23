namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception lancée quand une ressource demandée n'est pas trouvée
/// </summary>
public class NotFoundException : CommunityException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base(message, 404, innerException)
    {
    }
}
