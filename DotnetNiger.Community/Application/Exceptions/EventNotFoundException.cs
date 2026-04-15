namespace DotnetNiger.Community.Application.Exceptions;

/// <summary>
/// Exception lancée quand un événement n'est pas trouvé
/// </summary>
public class EventNotFoundException : NotFoundException
{
    public EventNotFoundException(string eventId)
        : base($"Événement avec l'ID '{eventId}' n'a pas été trouvé.")
    {
    }

    public EventNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
