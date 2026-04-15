namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception lancée quand un post n'est pas trouvé
/// </summary>
public class PostNotFoundException : NotFoundException
{
    public PostNotFoundException(string postId)
        : base($"Post avec l'ID '{postId}' n'a pas été trouvé.")
    {
    }

    public PostNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
