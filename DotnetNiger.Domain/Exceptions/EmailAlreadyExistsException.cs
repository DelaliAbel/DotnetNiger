namespace DotnetNiger.Domain.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email)
        : base($"Un compte avec l'email '{email}' existe déjà.")
    {
    }

    public EmailAlreadyExistsException(string email, string message)
        : base(message)
    {
    }
}
