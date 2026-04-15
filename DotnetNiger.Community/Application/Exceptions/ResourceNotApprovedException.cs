namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception lancée quand une ressource n'a pas été approuvée
/// </summary>
public class ResourceNotApprovedException : CommunityException
{
    public ResourceNotApprovedException(string resourceId)
        : base($"Ressource avec l'ID '{resourceId}' n'a pas été approuvée ou n'existe pas.", 403)
    {
    }

    public ResourceNotApprovedException(string message, Exception innerException)
        : base(message, 403, innerException)
    {
    }
}
