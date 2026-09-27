namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Échec d'acheminement d'un message électronique.
/// </summary>
/// <remarks>
/// Introduite pour que l'appelant puisse distinguer « le courriel n'est pas parti » d'une
/// panne de son propre traitement, sans avoir à connaître la bibliothèque SMTP employée.
/// Masquer l'erreur serait pire : l'appelant annoncerait alors à un membre qu'un message a
/// été envoyé alors qu'il ne l'a pas été.
/// </remarks>
public sealed class EmailDeliveryException : Exception
{
    /// <summary>
    /// Crée une erreur d'acheminement.
    /// </summary>
    /// <param name="message">Description de l'échec.</param>
    public EmailDeliveryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Crée une erreur d'acheminement à partir d'une exception du transport.
    /// </summary>
    /// <param name="message">Description de l'échec.</param>
    /// <param name="innerException">Exception d'origine.</param>
    public EmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
