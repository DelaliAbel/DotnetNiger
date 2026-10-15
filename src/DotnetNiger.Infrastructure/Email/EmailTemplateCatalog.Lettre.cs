namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles de la lettre d'information : confirmation, désinscription, nouveau contenu.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Fabrique les modèles de la lettre d'information.
    /// </summary>
    /// <returns>Les modèles, dans l'ordre d'affirmation de l'administration.</returns>
    private static EmailTemplate[] ModelesDeLaLettre() =>
    [
        new EmailTemplate(
            EmailTemplateKeys.NewsletterConfirmation,
            "Confirmation d'abonnement à la lettre",
            "Envoyé à l'inscription, avec la confirmation exigée par la loi sur le courriel.",
            "Vous avez demandé à recevoir la lettre d'information de DotnetNiger.",
            [EmailVariables.UrlDesabonnement],
            _ => "Confirmez votre abonnement à la lettre d'information",
            valeurs => new EmailContent(
                "Confirmez votre abonnement",
                [
                    "Vous avez demandé à recevoir la lettre d'information de DotnetNiger : un résumé des événements, ressources, articles et projets de la communauté.",
                    "Confirmez votre demande pour commencer à recevoir ces messages.",
                ],
                new EmailAction(
                    "Confirmer mon abonnement",
                    valeurs.Lien(EmailVariables.UrlDesabonnement)),
                "Si vous ne souhaitez pas recevoir ces messages, il suffit de ne pas confirmer : aucune adresse ne sera enregistrée."),
            EmailValues.De(
                (EmailVariables.UrlDesabonnement, "https://dotnetniger.test/lettre-information/confirmer?jeton=apercu"))),

        new EmailTemplate(
            EmailTemplateKeys.NewsletterUnsubscribed,
            "Désinscription confirmée",
            "Envoyé pour confirmer qu'une désinscription a bien été prise en compte.",
            "Vous avez demandé à ne plus recevoir la lettre d'information de DotnetNiger.",
            [EmailVariables.UrlCompte],
            _ => "Vous n'êtes plus inscrit à la lettre d'information",
            valeurs => new EmailContent(
                "Désinscription enregistrée",
                [
                    "Votre adresse a été retirée de la lettre d'information. Vous ne recevrez plus ces messages.",
                    "Votre compte reste actif, et vous pouvez vous réinscrire à tout moment depuis votre profil.",
                ],
                new EmailAction("Ouvrir mon compte", valeurs.Lien(EmailVariables.UrlCompte))),
            EmailValues.De(
                (EmailVariables.UrlCompte, "https://dotnetniger.test/compte"))),

        new EmailTemplate(
            EmailTemplateKeys.NewsletterNewContent,
            "Nouveau contenu publié",
            "Envoyé à chaque abonné lorsqu'un événement, un article, une ressource ou un projet est publié.",
            "Vous êtes abonné à la lettre d'information de DotnetNiger.",
            [
                EmailVariables.TypeContenu,
                EmailVariables.TitreContenu,
                EmailVariables.ResumeContenu,
                EmailVariables.UrlContenu,
                EmailVariables.UrlDesabonnement,
            ],
            valeurs =>
                $"{valeurs.Texte(EmailVariables.TypeContenu)} sur DotnetNiger : {valeurs.Texte(EmailVariables.TitreContenu)}",
            valeurs => new EmailContent(
                $"Un nouveau {valeurs.Texte(EmailVariables.TypeContenu).ToLowerInvariant()} est en ligne",
                [
                    valeurs.Texte(EmailVariables.ResumeContenu),
                ],
                new EmailAction("Lire la suite", valeurs.Lien(EmailVariables.UrlContenu)),
                UrlDesinscription: valeurs.Texte(EmailVariables.UrlDesabonnement)),
            EmailValues.De(
                (EmailVariables.TypeContenu, "Événement"),
                (EmailVariables.TitreContenu, "Rencontre .NET de Niamey"),
                (EmailVariables.ResumeContenu, "Une soirée d'échange sur ASP.NET Core, ouverte aux développeurs de tout niveau."),
                (EmailVariables.UrlContenu, "https://dotnetniger.test/evenements/rencontre-dotnet"),
                (EmailVariables.UrlDesabonnement, "https://dotnetniger.test/lettre-information/desinscription?jeton=apercu"))),
    ];
}