using System;

namespace DotnetNiger.Common.Exceptions;

/// <summary>
/// Levée lorsqu'un slug (URL) est déjà utilisé par une autre entité.
/// </summary>
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
