namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles du formulaire de contact : accusé de réception et transmission à l'administration.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Fabrique les modèles de contact.
    /// </summary>
    /// <returns>Les modèles, dans l'ordre d'affirmation de l'administration.</returns>
    private static EmailTemplate[] ModelesDeContact() =>
    [
        new EmailTemplate(
            EmailTemplateKeys.ContactAcknowledged,
            "Accusé de réception d'un message",
            "Envoyé à la personne qui a écrit via le formulaire de contact.",
            "Vous avez écrit à l'administration de DotnetNiger.",
            [EmailVariables.NomContact, EmailVariables.SujetContact, EmailVariables.MessageContact],
            valeurs => $"Message reçu : {valeurs.Texte(EmailVariables.SujetContact)}",
            valeurs => new EmailContent(
                "Nous avons bien reçu votre message",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomContact)},",
                    "Merci de nous avoir écrit. Un membre de l'équipe vous répondra dans les meilleurs délais.",
                    $"Votre message : « {valeurs.Texte(EmailVariables.SujetContact)} »",
                    // Le corps est recopié tel quel : c'est la seule preuve, sans ouvrir la boîte
                    // de réception, que rien n'a été tronqué en route.
                    valeurs.Texte(EmailVariables.MessageContact),
                ],
                Note: "Cet accusé est envoyé automatiquement, n'y répondez pas."),
            EmailValues.De(
                (EmailVariables.NomContact, "Awa Traoré"),
                (EmailVariables.SujetContact, "Proposer un atelier pour les étudiants"),
                (EmailVariables.MessageContact, "Bonjour, je souhaite animer un atelier d'initiation à C# à l'université Abdou Moumouni."))),

        new EmailTemplate(
            EmailTemplateKeys.ContactForwarded,
            "Nouveau message de contact",
            "Envoyé à l'administration, avec le message reçu et les coordonnées de son auteur.",
            "Un membre a utilisé le formulaire de contact de DotnetNiger.",
            [
                EmailVariables.NomContact,
                EmailVariables.EmailContact,
                EmailVariables.SujetContact,
                EmailVariables.MessageContact,
            ],
            valeurs => $"Message de contact : {valeurs.Texte(EmailVariables.SujetContact)}",
            valeurs => new EmailContent(
                "Un message arrive par le formulaire de contact",
                [
                    $"De : {valeurs.Texte(EmailVariables.NomContact)} <{valeurs.Texte(EmailVariables.EmailContact)}>",
                    $"Objet : {valeurs.Texte(EmailVariables.SujetContact)}",
                    valeurs.Texte(EmailVariables.MessageContact),
                ],
                Note: "Répondez directement à l'adresse de l'expéditeur."),
            EmailValues.De(
                (EmailVariables.NomContact, "Awa Traoré"),
                (EmailVariables.EmailContact, "awa.traore@exemple.test"),
                (EmailVariables.SujetContact, "Proposer un atelier pour les étudiants"),
                (EmailVariables.MessageContact, "Bonjour, je souhaite animer un atelier d'initiation à C# à l'université Abdou Moumouni."))),
    ];
}