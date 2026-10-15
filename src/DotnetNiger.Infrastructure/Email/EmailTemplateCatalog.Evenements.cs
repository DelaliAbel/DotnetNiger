namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Modèles liés aux événements : inscription, liste d'attente, promotion, annulation, modification.
/// </summary>
public static partial class EmailTemplateCatalog
{
    /// <summary>
    /// Fabrique les modèles des événements.
    /// </summary>
    /// <returns>Les modèles, dans l'ordre d'affirmation de l'administration.</returns>
    private static EmailTemplate[] ModelesDesEvenements() =>
    [
        new EmailTemplate(
            EmailTemplateKeys.EventRegistrationConfirmed,
            "Inscription à un événement",
            "Envoyé lorsqu'une place est réservée.",
            "Vous vous êtes inscrit à un événement sur DotnetNiger.",
            [
                EmailVariables.NomMembre,
                EmailVariables.TitreEvenement,
                EmailVariables.DateEvenement,
                EmailVariables.LieuEvenement,
                EmailVariables.UrlEvenement,
            ],
            valeurs => $"Inscription confirmée : {valeurs.Texte(EmailVariables.TitreEvenement)}",
            valeurs => new EmailContent(
                "Votre place est réservée",
                [
                    $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                    $"La date est fixée au {valeurs.Texte(EmailVariables.DateEvenement)}.",
                    $"Lieu : {valeurs.Texte(EmailVariables.LieuEvenement)}",
                ],
                new EmailAction("Voir l'événement", valeurs.Lien(EmailVariables.UrlEvenement)),
                "Si vous ne pouvez plus venir, désinscrivez-vous depuis la page de l'événement afin de libérer votre place pour un autre membre."),
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa Traoré"),
                (EmailVariables.TitreEvenement, "Rencontre .NET de Niamey"),
                (EmailVariables.DateEvenement, "samedi 18 mars 2026 à 16 h"),
                (EmailVariables.LieuEvenement, "salle polyvalente de l'université Abdou Moumouni, Niamey"),
                (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet"))),

        new EmailTemplate(
            EmailTemplateKeys.EventRegistrationWaitlisted,
            "Inscription en liste d'attente",
            "Envoyé lorsque l'événement est complet, pour signaler que la demande est retenue.",
            "Vous avez demandé une place pour un événement complet sur DotnetNiger.",
            [
                EmailVariables.TitreEvenement,
                EmailVariables.UrlEvenement,
                EmailVariables.UrlDesinscriptionEvenement,
            ],
            valeurs => $"Complet : {valeurs.Texte(EmailVariables.TitreEvenement)}",
            valeurs => new EmailContent(
                "Événement complet, votre demande est retenue",
                [
                    $"Il n'y a plus de place disponible pour {valeurs.Texte(EmailVariables.TitreEvenement)}.",
                    "Vous êtes inscrit sur la liste d'attente. Si quelqu'un se désinscrit, vous serez prévenu le premier et pourrez confirmer votre place.",
                ],
                new EmailAction("Suivre l'événement", valeurs.Lien(EmailVariables.UrlEvenement)),
                "Si vous ne souhaitez plus attendre, retirez votre demande à tout moment.",
                valeurs.Texte(EmailVariables.UrlDesinscriptionEvenement)),
            EmailValues.De(
                (EmailVariables.TitreEvenement, "Rencontre .NET de Niamey"),
                (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet"),
                (EmailVariables.UrlDesinscriptionEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet/inscriptions?jeton=apercu"))),

        new EmailTemplate(
            EmailTemplateKeys.EventWaitlistPromoted,
            "Une place s'est libérée",
            "Envoyé à la personne suivante de la liste d'attente, pour lui laisser confirmer sa place.",
            "Vous êtes sur la liste d'attente d'un événement DotnetNiger.",
            [EmailVariables.TitreEvenement, EmailVariables.UrlEvenement],
            valeurs => $"Une place se libère pour {valeurs.Texte(EmailVariables.TitreEvenement)}",
            valeurs => new EmailContent(
                "Une place vous est réservée",
                [
                    $"Une place s'est libérée pour {valeurs.Texte(EmailVariables.TitreEvenement)}.",
                    "Elle vous revient automatiquement, au titre de première personne sur la liste d'attente.",
                ],
                new EmailAction("Voir l'événement", valeurs.Lien(EmailVariables.UrlEvenement)),
                "Si vous ne pouvez plus venir, désinscrivez-vous : la place ira directement au membre suivant."),
            EmailValues.De(
                (EmailVariables.TitreEvenement, "Rencontre .NET de Niamey"),
                (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet"))),

        new EmailTemplate(
            EmailTemplateKeys.EventCancelled,
            "Événement annulé",
            "Envoyé à tous les inscrits lorsqu'un événement est annulé.",
            "Vous êtes inscrit à un événement sur DotnetNiger.",
            [EmailVariables.TitreEvenement, EmailVariables.DateEvenement],
            valeurs => $"Annulé : {valeurs.Texte(EmailVariables.TitreEvenement)}",
            valeurs => new EmailContent(
                "Cet événement est annulé",
                [
                    $"{valeurs.Texte(EmailVariables.TitreEvenement)} n'aura pas lieu.",
                    $"Il était prévu le {valeurs.Texte(EmailVariables.DateEvenement)}. L'équipe reste à votre disposition pour proposer une nouvelle date.",
                ],
                Note: "Votre inscription a été retirée de la liste des participants."),
            EmailValues.De(
                (EmailVariables.TitreEvenement, "Rencontre .NET de Niamey"),
                (EmailVariables.DateEvenement, "samedi 18 mars 2026 à 16 h"))),

        new EmailTemplate(
            EmailTemplateKeys.EventUpdated,
            "Événement modifié",
            "Envoyé aux inscrits lorsque la date, le lieu ou le programme change.",
            "Vous êtes inscrit à un événement sur DotnetNiger.",
            [
                EmailVariables.TitreEvenement,
                EmailVariables.DateEvenement,
                EmailVariables.LieuEvenement,
                EmailVariables.UrlEvenement,
            ],
            valeurs => $"Modifié : {valeurs.Texte(EmailVariables.TitreEvenement)}",
            valeurs => new EmailContent(
                "Un événement que vous suivez a changé",
                [
                    $"Les informations de {valeurs.Texte(EmailVariables.TitreEvenement)} ont été mises à jour.",
                    $"Date : {valeurs.Texte(EmailVariables.DateEvenement)}",
                    $"Lieu : {valeurs.Texte(EmailVariables.LieuEvenement)}",
                ],
                new EmailAction("Voir les détails", valeurs.Lien(EmailVariables.UrlEvenement))),
            EmailValues.De(
                (EmailVariables.TitreEvenement, "Rencontre .NET de Niamey"),
                (EmailVariables.DateEvenement, "samedi 18 mars 2026 à 18 h"),
                (EmailVariables.LieuEvenement, "salle polyvalente de l'université Abdou Moumouni, Niamey"),
                (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet"))),
    ];
}