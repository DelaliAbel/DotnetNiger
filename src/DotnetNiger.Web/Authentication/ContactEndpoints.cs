using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Dépôt des messages du formulaire de contact.
/// </summary>
/// <remarks>
/// Le message est enregistré <em>et</em> transmis par courriel, dans cet ordre. Un
/// courriel peut être avalé par un filtre ou perdu : sans trace en
/// base, un visiteur qui attend une réponse n'aurait aucun moyen de le signaler, et
/// l'administration ne retrouverait rien. L'ordre inverse, n'envoyer que si l'écriture
/// a réussi, se ferait en une transaction : le message disparaîtrait alors si la file
/// d'envoi tombait à l'instant de l'enregistrement.
/// </remarks>
public static class ContactEndpoints
{
    /// <summary>
    /// Déclare le point de terminaison du formulaire de contact.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    public static WebApplication MapContactEndpoints(this WebApplication app)
    {
        // Un formulaire ne se soumet qu'une fois, mais rien n'interdit de rejouer la requête.
        // Le compteur de limitation de débit, posé en amont dans le pipeline, borne ce cas
        // comme tous les autres envois répétés.
        app.MapPost("/contact/envoyer", async (
            [FromForm] ContactInput input,
            IUnitOfWork unitOfWork,
            IEmailDispatcher emails,
            IOptions<EmailOptions> emailOptions,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var nom = input.Nom?.Trim() ?? string.Empty;
            var courriel = input.Email?.Trim() ?? string.Empty;
            var sujet = input.Sujet?.Trim() ?? string.Empty;
            var corps = input.Message?.Trim() ?? string.Empty;

            if (!Valide(nom, courriel, sujet, corps, out var erreur))
            {
                ContactLog.RequeteRefusee(logger);

                return Results.Redirect(
                    $"/contact?erreur={Uri.EscapeDataString(erreur)}#formulaire");
            }

            var message = ContactMessage.Create(nom, courriel, sujet, corps, DateTime.UtcNow);
            unitOfWork.Repository<ContactMessage>().Add(message);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Deux courriels, deux lecteurs. L'accusé va au visiteur : il sait que son
            // message est arrivé et qu'une réponse partira. La copie va à l'équipe, à
            // l'adresse de contact configurée : c'est elle qui rend le message
            // traitable. Les deux gabarits recopient le corps, seule preuve que rien
            // n'a été tronqué en route.
            var valeurs = EmailValues.De(
                (EmailVariables.NomContact, nom),
                (EmailVariables.EmailContact, courriel),
                (EmailVariables.SujetContact, sujet),
                (EmailVariables.MessageContact, corps));

            await emails.DispatchAsync(
                EmailTemplateKeys.ContactAcknowledged, courriel, valeurs, cancellationToken);

            var adresseEquipe = emailOptions.Value.ContactAddress;
            if (!string.IsNullOrWhiteSpace(adresseEquipe))
            {
                await emails.DispatchAsync(
                    EmailTemplateKeys.ContactForwarded, adresseEquipe, valeurs, cancellationToken);
            }
            else
            {
                // Sans adresse de contact configurée, le message reste en base et
                // l'administration le retrouve au tableau de bord. Aucun envoi ne part,
                // mais rien n'est perdu : c'est la raison d'enregistrer avant d'envoyer.
                ContactLog.AdresseEquipeAbsente(logger);
            }

            ContactLog.MessageDepose(logger);

            return Results.Redirect("/contact?envoye=1#formulaire");
        });

        return app;
    }

    /// <summary>
    /// Vérifie les champs saisis avant d'écrire quoi que ce soit.
    /// </summary>
    /// <remarks>
    /// La validation reste ici et non dans l'entité : celle-ci lève une exception, qui
    /// remonterait jusqu'au gestionnaire d'erreurs en renvoyant une page technique à la
    /// place du formulaire. Un formulaire doit répondre par un message lisible, et
    /// nommer le champ fautif.
    /// </remarks>
    /// <param name="nom">Nom saisi.</param>
    /// <param name="courriel">Adresse saisie.</param>
    /// <param name="sujet">Objet saisi.</param>
    /// <param name="corps">Message saisi.</param>
    /// <param name="erreur">Premier problème rencontré, ou <see langword="null"/>.</param>
    /// <returns><see langword="true"/> si le message peut être enregistré.</returns>
    private static bool Valide(string nom, string courriel, string sujet, string corps, out string erreur)
    {
        if (nom.Length is 0 or > FieldLimits.Name)
        {
            erreur = "Indiquez votre nom.";
            return false;
        }

        if (courriel.Length is 0 or > FieldLimits.EmailAddress
            || !courriel.Contains('@', StringComparison.Ordinal)
            || courriel.StartsWith('@') || courriel.EndsWith('@'))
        {
            erreur = "Indiquez une adresse e-mail valide, c'est elle qui recevra la réponse.";
            return false;
        }

        if (sujet.Length is 0 or > FieldLimits.Subject)
        {
            erreur = "Indiquez l'objet de votre message.";
            return false;
        }

        if (corps.Length is 0 or > FieldLimits.Content)
        {
            erreur = "Votre message est trop court ou trop long.";
            return false;
        }

        erreur = string.Empty;
        return true;
    }

    /// <summary>Champs transmis par le formulaire de contact.</summary>
    /// <param name="Nom">Nom de l'auteur du message.</param>
    /// <param name="Email">Adresse à laquelle répondre.</param>
    /// <param name="Sujet">Objet du message.</param>
    /// <param name="Message">Corps du message.</param>
    public sealed record ContactInput(string? Nom, string? Email, string? Sujet, string? Message);
}

/// <summary>Journalisation du formulaire de contact.</summary>
internal static partial class ContactLog
{
    // Ni le contenu du message ni son auteur n'arrivent au journal : ce sont des
    // données personnelles, et le journal n'a pas à être une seconde copie.
    [LoggerMessage(EventId = 150, Level = LogLevel.Information,
        Message = "Message de contact depose")]
    public static partial void MessageDepose(ILogger logger);

    [LoggerMessage(EventId = 151, Level = LogLevel.Debug,
        Message = "Message de contact refuse a la validation du formulaire")]
    public static partial void RequeteRefusee(ILogger logger);

    [LoggerMessage(EventId = 152, Level = LogLevel.Warning,
        Message = "Aucune adresse de contact configuree : le message reste en base, aucun courriel ne part vers l'equipe")]
    public static partial void AdresseEquipeAbsente(ILogger logger);
}
