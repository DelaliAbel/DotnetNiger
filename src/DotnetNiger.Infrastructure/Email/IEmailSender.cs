namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Envoi d'un message électronique.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Expédie un message.
    /// </summary>
    /// <param name="recipient">Adresse du destinataire.</param>
    /// <param name="message">Objet et corps du message.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant l'envoi.</returns>
    /// <remarks>
    /// Une erreur d'envoi n'est jamais masquée : l'appelant doit savoir qu'un message n'est
    /// pas parti, sinon il annoncerait à un membre qu'un courriel a été envoyé.
    /// </remarks>
    Task SendAsync(
        string recipient,
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
