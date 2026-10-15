using DotnetNiger.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un réglage du site, tel que l'administration le consulte.</summary>
/// <param name="Intitule">Nom du réglage.</param>
/// <param name="Valeur">Valeur en vigueur, ou l'explication de son absence.</param>
/// <param name="EstComplet">Faux quand la valeur manque ou est incohérente.</param>
public sealed record LigneReglage(string Intitule, string Valeur, bool EstComplet = true);

/// <summary>Un modèle de courriel, essayable depuis la page.</summary>
/// <param name="Cle">Clé du modèle, telle que celle du serveur de courriel.</param>
/// <param name="Intitule">Ce que le modèle sert à faire.</param>
public sealed record ModeleCourriel(string Cle, string Intitule);

/// <summary>Configuration lue pour la page Paramètres.</summary>
/// <param name="Reglages">Réglages du site et du courriel.</param>
/// <param name="Modeles">Modèles de courriel.</param>
/// <param name="EnvoiPossible">Vrai si le site peut réellement envoyer des courriels.</param>
/// <param name="Manquants">Réglages absents qui bloquent l'envoi.</param>
public sealed record ParametresVue(
    IReadOnlyList<LigneReglage> Reglages,
    IReadOnlyList<ModeleCourriel> Modeles,
    bool EnvoiPossible,
    IReadOnlyList<string> Manquants);

/// <summary>
/// Lecture de la configuration pour la page Paramètres.
/// </summary>
/// <remarks>
/// Les réglages sont lus, jamais écrits : la configuration vit dans les fichiers de
/// déploiement, pas en base. La faire passer par un formulaire donnerait l'illusion que
/// l'adresse d'envoi se change depuis le site — elle ne se change pas, et une valeur
/// modifiée à chaud serait perdue au prochain redémarrage.
/// <para>
/// Le mot de passe du serveur de courriel n'est jamais lu et n'apparaît donc pas dans la
/// liste. Seul son état — présent ou absent — est utile, et c'est ce que la page montre.
/// </para>
/// </remarks>
public sealed class ServiceParametres(IOptions<EmailOptions> options)
{
    /// <summary>Lit la configuration en vigueur.</summary>
    /// <returns>Les réglages, les modèles de courriel et l'état de l'envoi.</returns>
    public ParametresVue Lire()
    {
        var courriel = options.Value;

        return new ParametresVue(
            [
                new("Adresse du site", Valeur(courriel.SiteUrl), !Absent(courriel.SiteUrl)),
                new("Adresse de contact", Valeur(courriel.ContactAddress), !Absent(courriel.ContactAddress)),
                new(
                    "Expéditeur",
                    $"{courriel.FromDisplayName} <{Valeur(courriel.FromAddress)}>",
                    !Absent(courriel.FromAddress)),
                new(
                    "Adresse de réponse",
                    string.IsNullOrWhiteSpace(courriel.ReplyToAddress)
                        ? "celle de l'expéditeur"
                        : courriel.ReplyToAddress,
                    true),
                new(
                    "Serveur de courriel",
                    string.IsNullOrWhiteSpace(courriel.Host)
                        ? NonRenseigne
                        : $"{courriel.Host}:{courriel.Port} — {(courriel.UseSsl ? "chiffrement TLS" : "sans chiffrement")}",
                    !Absent(courriel.Host)),
                new("Authentification", Authentication(courriel), true),
                new(
                    "File d'attente",
                    "Les courriels partent par la file : un serveur momentanément injoignable ne perd aucun message."),
            ],
            [
                new(EmailTemplateKeys.AccountConfirmation, "Confirmer l'adresse d'un compte"),
                new(EmailTemplateKeys.AccountWelcome, "Accueillir un nouveau membre"),
                new(EmailTemplateKeys.AccountPasswordReset, "Réinitialiser un mot de passe"),
                new(EmailTemplateKeys.AccountDeletionConfirmation, "Confirmer la suppression d'un compte"),
                new(EmailTemplateKeys.NewsletterConfirmation, "Confirmer un abonnement à la lettre"),
                new(EmailTemplateKeys.NewsletterUnsubscribed, "Confirmer un désabonnement de la lettre"),
                new(EmailTemplateKeys.NewsletterNewContent, "Annoncer une publication dans la lettre"),
                new(EmailTemplateKeys.EventRegistrationConfirmed, "Confirmer une inscription à un événement"),
                new(EmailTemplateKeys.EventRegistrationWaitlisted, "Annoncer une place en attente"),
                new(EmailTemplateKeys.EventWaitlistPromoted, "Annoncer une place finalement libre"),
                new(EmailTemplateKeys.EventCancelled, "Annoncer l'annulation d'un événement"),
                new(EmailTemplateKeys.EventUpdated, "Annoncer une modification d'un événement"),
                new(EmailTemplateKeys.CommentPosted, "Signaler un nouveau commentaire à l'équipe"),
                new(EmailTemplateKeys.ContentRepublished, "Signaler une republication à l'équipe"),
                new(EmailTemplateKeys.ContactAcknowledged, "Accuser réception d'un message de contact"),
                new(EmailTemplateKeys.ContactForwarded, "Transmettre un message de contact à l'équipe"),
            ],
            courriel.IsConfigured,
            courriel.DescribeMissingSettings());
    }

    /// <summary>Texte affiché pour une valeur absente.</summary>
    private const string NonRenseigne = "non renseignée";

    /// <summary>Décrit l'authentification du serveur de courriel sans révéler le secret.</summary>
    /// <param name="courriel">Configuration du courriel.</param>
    /// <returns>Une description de l'état de l'authentification.</returns>
    private static string Authentication(EmailOptions courriel)
    {
        if (string.IsNullOrWhiteSpace(courriel.UserName))
        {
            return "aucune — le serveur n'exige pas d'authentification";
        }

        // Un identifiant sans mot de passe, ou l'inverse, est une configuration cassée :
        // le serveur refusera l'envoi sans que l'erreur ne le dise clairement.
        return string.IsNullOrWhiteSpace(courriel.Password)
            ? $"{courriel.UserName} — mot de passe manquant, l'envoi échouera"
            : $"{courriel.UserName} — mot de passe renseigné";
    }

    /// <summary>Normalise une valeur pour l'affichage.</summary>
    /// <param name="valeur">Valeur de configuration.</param>
    /// <returns>La valeur, ou l'explication de son absence.</returns>
    private static string Valeur(string? valeur)
        => string.IsNullOrWhiteSpace(valeur) ? NonRenseigne : valeur;

    /// <summary>Indique si une valeur de configuration est absente.</summary>
    /// <param name="valeur">Valeur de configuration.</param>
    /// <returns><see langword="true"/> si la valeur est absente.</returns>
    private static bool Absent(string? valeur) => string.IsNullOrWhiteSpace(valeur);
}