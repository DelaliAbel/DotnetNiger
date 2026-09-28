using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Rend un modèle du catalogue et le met en file d'attente.
/// </summary>
/// <remarks>
/// Chaque point de terminaison qui prévient un membre réécrirait sinon la même séquence :
/// chercher le modèle, vérifier la destination, composer l'identité du site, rendre, mettre
/// en file. Les étapes comportent des pièges — une adresse absente ne doit pas faire échouer
/// l'inscription, un modèle inconnu ne doit pas partir vide — et les recopierGuarantee
/// qu'un seul d'entre eux soit corrigé un jour. Elles sont donc réunies ici.
/// </remarks>
public interface IEmailDispatcher
{
    /// <summary>
    /// Rend un modèle et le met en file pour une adresse connue.
    /// </summary>
    /// <param name="templateKey">Clé du modèle dans le catalogue.</param>
    /// <param name="recipient">Adresse du destinataire.</param>
    /// <param name="values">Valeurs du modèle.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si le message a été mis en file.</returns>
    Task<bool> DispatchAsync(
        string templateKey,
        string recipient,
        EmailValues values,
        CancellationToken cancellationToken);

    /// <summary>
    /// Rend un modèle et le met en file pour un membre, en ignorant l'absence d'adresse.
    /// </summary>
    /// <param name="templateKey">Clé du modèle dans le catalogue.</param>
    /// <param name="memberId">Identifiant du membre destinataire.</param>
    /// <param name="values">Valeurs du modèle.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si le message a été mis en file.</returns>
    /// <remarks>
    /// C'est la forme que les parcours métier utilisent. Un membre sans adresse ne doit pas
    /// faire échouer son inscription à un événement : le message est simplement abandonné,
    /// et le fait est consigné pour que l'absence ne passe pas inaperçue.
    /// <para>
    /// Le nom affiché n'est pas transmis automatiquement : seul le modèle concerné le
    /// demande, et le contrôleur qui l'a sous les yeux le fournit dans ses valeurs.
    /// </para>
    /// </remarks>
    Task<bool> DispatchAsync(
        string templateKey,
        Guid memberId,
        EmailValues values,
        CancellationToken cancellationToken);
}

/// <inheritdoc cref="IEmailDispatcher" />
public sealed partial class EmailDispatcher(
    IOutbox outbox,
    IMemberDirectory members,
    IOptions<EmailOptions> options,
    ILogger<EmailDispatcher> logger) : IEmailDispatcher
{
    /// <inheritdoc />
    public async Task<bool> DispatchAsync(
        string templateKey,
        string recipient,
        EmailValues values,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            DestinataireAbsent(logger, templateKey);
            return false;
        }

        var modele = EmailTemplateCatalog.Obtenir(templateKey);
        var message = modele.Rendre(values, EmailBranding.DepuisOptions(options.Value));

        await outbox.EnqueueAsync(templateKey, new QueuedEmail(recipient, message), cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DispatchAsync(
        string templateKey,
        Guid memberId,
        EmailValues values,
        CancellationToken cancellationToken)
    {
        var contact = await members.ObtenirAsync(memberId, cancellationToken);

        return await DispatchAsync(templateKey, contact?.Email!, values, cancellationToken);
    }

    [LoggerMessage(
        EventId = 90,
        Level = LogLevel.Warning,
        Message = "Le modèle {Modele} n'a pas pu être mis en file : le membre n'a pas d'adresse")]
    private static partial void DestinataireAbsent(ILogger logger, string modele);
}
