namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles du cycle de vie d'un compte : confirmation, bienvenue, mot de passe, suppression.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Fabrique les modèles du compte.
    /// </summary>
    /// <returns>Les modèles, dans l'ordre d'affirmation de l'administration.</returns>
    private static EmailTemplate[] ModelesDuCompte() =>
    [
        new EmailTemplate(
            EmailTemplateKeys.AccountConfirmation,
            "Confirmation d'inscription",
            "Envoyé juste après l'inscription, pour prouver que l'adresse est bien à vous.",
            "Vous avez créé un compte sur DotnetNiger.",
            [EmailVariables.NomMembre, EmailVariables.UrlConfirmation],
            _ => "Confirmez votre adresse sur DotnetNiger",
            valeurs => new EmailContent(
                "Confirmez votre adresse",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                    "Merci de votre inscription. Confirmez votre adresse pour activer votre compte, puis vous pourrez commenter, publier des ressources et vous inscrire aux événements.",
                ],
                new EmailAction(
                    "Confirmer mon adresse",
                    valeurs.Lien(EmailVariables.UrlConfirmation)),
                "Si vous n'êtes pas à l'origine de cette inscription, ignorez ce message."),
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa Traoré"),
                (EmailVariables.UrlConfirmation, "https://dotnetniger.test/compte/confirmer-email?jeton=apercu"))),

        new EmailTemplate(
            EmailTemplateKeys.AccountWelcome,
            "Bienvenue dans la communauté",
            "Envoyé une fois l'adresse confirmée.",
            "Vous avez confirmé l'adresse de votre compte DotnetNiger.",
            [EmailVariables.NomMembre, EmailVariables.UrlCompte],
            _ => "Bienvenue sur DotnetNiger",
            valeurs => new EmailContent(
                "Votre compte est actif",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                    "Votre adresse est confirmée. La communauté DotnetNiger réunit les développeurs .NET du Niger autour d'événements, de ressources et de projets open source.",
                ],
                new EmailAction("Découvrir la communauté", valeurs.Lien(EmailVariables.UrlCompte)),
                "L'inscription à la lettre d'information reste facultative."),
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa Traoré"),
                (EmailVariables.UrlCompte, "https://dotnetniger.test/compte"))),

        new EmailTemplate(
            EmailTemplateKeys.AccountPasswordReset,
            "Réinitialisation du mot de passe",
            "Envoyé à la demande, quand un mot de passe a été oublié.",
            "Vous avez demandé à réinitialiser le mot de passe de votre compte DotnetNiger.",
            [EmailVariables.NomMembre, EmailVariables.UrlReinitialisation],
            _ => "Réinitialisez votre mot de passe DotnetNiger",
            valeurs => new EmailContent(
                "Réinitialisation de votre mot de passe",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                    "Une réinitialisation a été demandée pour votre compte.",
                ],
                new EmailAction(
                    "Choisir un nouveau mot de passe",
                    valeurs.Lien(EmailVariables.UrlReinitialisation)),
                "Ce lien expire. S'il ne vous correspond pas, ignorez ce message : votre mot de passe reste inchangé."),
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa Traoré"),
                (EmailVariables.UrlReinitialisation, "https://dotnetniger.test/compte/reinitialiser-mot-de-passe?courriel=awa%40exemple.ne&jeton=apercu"))),

        new EmailTemplate(
            EmailTemplateKeys.AccountDeletionConfirmation,
            "Suppression du compte",
            "Envoyé pour confirmer une suppression, qui n'a lieu qu'après deux confirmations.",
            "Vous avez demandé la suppression de votre compte DotnetNiger.",
            [
                EmailVariables.NomMembre,
                EmailVariables.UrlConfirmationSuppression,
                EmailVariables.DelaiSuppression,
            ],
            _ => "Confirmez la suppression de votre compte DotnetNiger",
            valeurs => new EmailContent(
                "Votre compte va être supprimé",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                    "Vous avez demandé la suppression définitive de votre compte. Vos publications, commentaires et inscriptions seront retirés.",
                    $"Sans confirmation de votre part, la suppression sera abandonnée sous {valeurs.Texte(EmailVariables.DelaiSuppression)}.",
                ],
                new EmailAction(
                    "Supprimer définitivement mon compte",
                    valeurs.Lien(EmailVariables.UrlConfirmationSuppression)),
                "Si vous n'êtes pas à l'origine de cette demande, ignorez ce message : votre compte reste actif."),
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa Traoré"),
                (EmailVariables.UrlConfirmationSuppression, "https://dotnetniger.test/compte/suppression?jeton=apercu"),
                (EmailVariables.DelaiSuppression, "48 heures"))),
    ];
}