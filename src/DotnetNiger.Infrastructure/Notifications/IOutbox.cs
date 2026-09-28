using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Infrastructure.Email;

namespace DotnetNiger.Infrastructure.Notifications;

/// <summary>
/// Courriel prêt à être mis en file, avec son destinataire.
/// </summary>
/// <param name="Recipient">Adresse du destinataire.</param>
/// <param name="Message">Objet et corps rendus.</param>
/// <remarks>
/// Le rendu a lieu avant l'appel : chaque destinataire d'une diffusion reçoit son propre
/// lien de désinscription, donc son propre corps.
/// </remarks>
public sealed record QueuedEmail(string Recipient, EmailMessage Message);

/// <summary>
/// Met les courriels en file d'attente au lieu de les expédier.
/// </summary>
/// <remarks>
/// Aucun appelant n'appelle le serveur SMTP. L'écriture se fait dans l'unité de travail de
/// l'appelant et n'est donc pas validée ici : la ligne n'est en base qu'après le
/// <c>SaveChangesAsync</c> du contrôleur, dans la même transaction que le changement
/// métier. Un message en base est un envoi garanti, un message absent est un envoi qui
/// n'aura jamais eu lieu.
/// </remarks>
public interface IOutbox
{
    /// <summary>
    /// Met un courriel en file.
    /// </summary>
    /// <param name="kind">Clé du modèle, voir <see cref="EmailTemplateKeys"/>.</param>
    /// <param name="email">Destinataire et corps déjà rendus.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche représentant l'opération.</returns>
    Task EnqueueAsync(string kind, QueuedEmail email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met plusieurs courriels en file pour une même diffusion.
    /// </summary>
    /// <param name="kind">Clé du modèle, voir <see cref="EmailTemplateKeys"/>.</param>
    /// <param name="emails">Destinataires et corps déjà rendus, un par abonné.</param>
    /// <param name="broadcastId">Diffusion à laquelle rattacher les messages.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Nombre de messages mis en file.</returns>
    Task<int> EnqueueManyAsync(
        string kind,
        IReadOnlyList<QueuedEmail> emails,
        Guid? broadcastId = null,
        CancellationToken cancellationToken = default);
}
