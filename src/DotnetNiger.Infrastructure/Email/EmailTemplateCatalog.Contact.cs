namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles du formulaire de contact : accusé de réception et transmission à l'administration.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Réponse rédigée par l'administration au demandeur.
    /// </summary>
    /// <remarks>
    /// Le message d'origine est rappelé sous la réponse : le demandeur lit souvent plusieurs
    /// échanges avant de se souvenir de ce qui avait motivé le sien, et l'objet seul ne
    /// suffit pas à le remettre en contexte.
    /// </remarks>
    private static EmailTemplate ModeleReponseContact() =>
        new(
            EmailTemplateKeys.ContactReplied,
            "Réponse de l'administration",
            "Envoyé à la personne qui a écrit, après réponse de l'administration.",
            "Vous avez répondu à un message envoyé à l'administration de DotnetNiger.",
            [EmailVariables.NomContact, EmailVariables.SujetContact, EmailVariables.MessageContact, EmailVariables.MessageReponse],
            valeurs => $"Re: {valeurs.Texte(EmailVariables.SujetContact)}",
            valeurs => new EmailContent(
                "Notre réponse",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomContact)},",
                    valeurs.Texte(EmailVariables.MessageReponse),
                    $"— Notre message : « {valeurs.Texte(EmailVariables.MessageContact)} »",
                ],
                Note: "Vous répondez à ce courriel et notre équipe vous lira."),
            EmailValues.De(
                (EmailVariables.NomContact, "Awa Traoré"),
                (EmailVariables.SujetContact, "Proposer un atelier pour les étudiants"),
                (EmailVariables.MessageContact, "Bonjour, je souhaite animer un atelier d'initiation à C# à l'université Abdou Moumouni."),
                (EmailVariables.MessageReponse, "Bonjour Awa, avec plaisir. Nous préparerons ensemble l'atelier, et je reviens vers vous cette semaine pour fixer la date.")));

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