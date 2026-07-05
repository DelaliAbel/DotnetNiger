using System;

namespace DotnetNiger.Common.Exceptions;

/// <summary>
/// Levée lorsqu'une adresse email est déjà utilisée par un autre utilisateur.
/// </summary>
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
