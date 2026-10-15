namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles liés au cycle de vie d'une publication : commentaire, remise en ligne.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Fabrique les modèles de contenu publié.
    /// </summary>
    /// <returns>Les modèles, dans l'ordre d'affirmation de l'administration.</returns>
    private static EmailTemplate[] ModelesDeContenu() =>
    [
        new EmailTemplate(
            EmailTemplateKeys.CommentPosted,
            "Nouveau commentaire",
            "Envoyé à l'auteur d'un contenu dès qu'un membre le commente.",
            "Vous publiez sur DotnetNiger.",
            [
                EmailVariables.TitreContenu,
                EmailVariables.UrlContenu,
                EmailVariables.AuteurCommentaire,
                EmailVariables.ExtraitCommentaire,
            ],
            valeurs => $"Nouveau commentaire sur {valeurs.Texte(EmailVariables.TitreContenu)}",
            valeurs => new EmailContent(
                "Quelqu'un a commenté votre publication",
                [
                    $"{valeurs.Texte(EmailVariables.AuteurCommentaire)} a commenté « {valeurs.Texte(EmailVariables.TitreContenu)} ».",
                    $"« {valeurs.Texte(EmailVariables.ExtraitCommentaire)} »",
                ],
                new EmailAction("Lire et répondre", valeurs.Lien(EmailVariables.UrlContenu))),
            EmailValues.De(
                (EmailVariables.TitreContenu, "Déboguer un interblocage"),
                (EmailVariables.UrlContenu, "https://dotnetniger.test/ressources/deboguer-un-interblocage"),
                (EmailVariables.AuteurCommentaire, "Ibrahim Sankara"),
                (EmailVariables.ExtraitCommentaire, "Merci pour l'explication, le graphique m'a beaucoup aidé."))),

        new EmailTemplate(
            EmailTemplateKeys.ContentRepublished,
            "Contenu remis en ligne",
            "Envoyé à l'auteur lorsqu'une de ses publications archivée redevient visible.",
            "Vous êtes l'auteur d'une publication sur DotnetNiger.",
            [
                EmailVariables.TypeContenu,
                EmailVariables.TitreContenu,
                EmailVariables.UrlContenu,
            ],
            valeurs => $"De nouveau en ligne : {valeurs.Texte(EmailVariables.TitreContenu)}",
            valeurs => new EmailContent(
                "Votre publication est de nouveau visible",
                [
                    $"L'administration a remis en ligne votre {valeurs.Texte(EmailVariables.TypeContenu).ToLowerInvariant()} « {valeurs.Texte(EmailVariables.TitreContenu)} ».",
                ],
                new EmailAction("Voir ma publication", valeurs.Lien(EmailVariables.UrlContenu)),
                "Si ce contenu ne vous convient plus, signalez-le à l'administration."),
            EmailValues.De(
                (EmailVariables.TypeContenu, "ressource"),
                (EmailVariables.TitreContenu, "Déboguer un interblocage"),
                (EmailVariables.UrlContenu, "https://dotnetniger.test/ressources/deboguer-un-interblocage"))),
    ];
}