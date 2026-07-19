namespace DotnetNiger.Domain.Exceptions;

public class SlugAlreadyExistsException : Exception
{
    public SlugAlreadyExistsException(string slug)
        : base($"Le slug '{slug}' est déjà utilisé.")
    {
    }

    public SlugAlreadyExistsException(string slug, string message)
        : base(message)
    {
    }
}
