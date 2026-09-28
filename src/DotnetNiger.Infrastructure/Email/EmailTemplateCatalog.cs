using System.Collections.Frozen;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Catalogue des modèles de courriel du site.
/// </summary>
/// <remarks>
/// Un seul endroit décrit les seize messages, avec leur objet, leur texte et un jeu de
/// valeurs d'exemple. L'administration s'en sert pour relire et essayer chaque modèle sans
/// provoquer d'envoi réel, et le code qui expédie passe par
/// <see cref="EmailTemplate.Rendre"/> plutôt que par une chaîne HTML collée dans le service
/// concerné : un message ne peut donc pas partir avec un rendu différent de celui qui a été
/// prévisualisé.
/// <para>
/// Les modèles vivent en C# et non en base : leur contenu fait partie du produit, il est
/// relu comme le code, et il n'a pas à être modifiable par un administrateur sans contrôle.
/// </para>
/// </remarks>
public static class EmailTemplateCatalog
{
    private static readonly EmailTemplate ConfirmationCompte = new(
        EmailTemplateKeys.AccountConfirmation,
        "Confirmation d'inscription",
        "Envoyé juste après l'inscription, pour prouver que l'adresse est bien à vous.",
        "Vous avez créé un compte sur dotnetNiger.",
        [EmailVariables.NomMembre, EmailVariables.UrlConfirmation],
        _ => "Confirmez votre adresse sur dotnetNiger",
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
            (EmailVariables.UrlConfirmation, "https://dotnetniger.test/compte/confirmer-email?jeton=apercu")));

    private static readonly EmailTemplate Bienvenue = new(
        EmailTemplateKeys.AccountWelcome,
        "Bienvenue dans la communauté",
        "Envoyé une fois l'adresse confirmée.",
        "Vous avez confirmé l'adresse de votre compte dotnetNiger.",
        [EmailVariables.NomMembre, EmailVariables.UrlCompte],
        _ => "Bienvenue sur dotnetNiger",
        valeurs => new EmailContent(
            "Votre compte est actif",
            [
                $"Bonjour {valeurs.Texte(EmailVariables.NomMembre)},",
                "Votre adresse est confirmée. La communauté dotNetNiger réunit les développeurs .NET du Niger autour d'événements, de ressources et de projets open source.",
            ],
            new EmailAction("Découvrir la communauté", valeurs.Lien(EmailVariables.UrlCompte)),
            "L'inscription à la lettre d'information reste facultative."),
        EmailValues.De(
            (EmailVariables.NomMembre, "Awa Traoré"),
            (EmailVariables.UrlCompte, "https://dotnetniger.test/compte")));

    private static readonly EmailTemplate Reinitialisation = new(
        EmailTemplateKeys.AccountPasswordReset,
        "Réinitialisation du mot de passe",
        "Envoyé à la demande, quand un mot de passe a été oublié.",
        "Vous avez demandé à réinitialiser le mot de passe de votre compte dotnetNiger.",
        [EmailVariables.NomMembre, EmailVariables.UrlReinitialisation],
        _ => "Réinitialisez votre mot de passe dotnetNiger",
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
            (EmailVariables.UrlReinitialisation, "https://dotnetniger.test/compte/mot-de-passe?jeton=apercu")));

    private static readonly EmailTemplate SuppressionCompte = new(
        EmailTemplateKeys.AccountDeletionConfirmation,
        "Suppression du compte",
        "Envoyé pour confirmer une suppression, qui n'a lieu qu'après deux confirmations.",
        "Vous avez demandé la suppression de votre compte dotnetNiger.",
        [
            EmailVariables.NomMembre,
            EmailVariables.UrlConfirmationSuppression,
            EmailVariables.DelaiSuppression,
        ],
        _ => "Confirmez la suppression de votre compte dotnetNiger",
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
            (EmailVariables.DelaiSuppression, "48 heures")));

    private static readonly EmailTemplate ConfirmationAbonnement = new(
        EmailTemplateKeys.NewsletterConfirmation,
        "Confirmation d'abonnement à la lettre",
        "Envoyé à l'inscription, avec la confirmation exigée par la loi sur le courriel.",
        "Vous avez demandé à recevoir la lettre d'information de dotnetNiger.",
        [EmailVariables.UrlDesabonnement],
        _ => "Confirmez votre abonnement à la lettre d'information",
        valeurs => new EmailContent(
            "Confirmez votre abonnement",
            [
                "Vous avez demandé à recevoir la lettre d'information de dotnetNiger : un résumé des événements, ressources, articles et projets de la communauté.",
                "Confirmez votre demande pour commencer à recevoir ces messages.",
            ],
            new EmailAction(
                "Confirmer mon abonnement",
                valeurs.Lien(EmailVariables.UrlDesabonnement)),
            "Si vous ne souhaitez pas recevoir ces messages, il suffit de ne pas confirmer : aucune adresse ne sera enregistrée."),
        EmailValues.De(
            (EmailVariables.UrlDesabonnement, "https://dotnetniger.test/lettre-information/confirmer?jeton=apercu")));

    private static readonly EmailTemplate Desabonnement = new(
        EmailTemplateKeys.NewsletterUnsubscribed,
        "Désinscription confirmée",
        "Envoyé pour confirmer qu'une désinscription a bien été prise en compte.",
        "Vous avez demandé à ne plus recevoir la lettre d'information de dotnetNiger.",
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
            (EmailVariables.UrlCompte, "https://dotnetniger.test/compte")));

    private static readonly EmailTemplate NouveauContenu = new(
        EmailTemplateKeys.NewsletterNewContent,
        "Nouveau contenu publié",
        "Envoyé à chaque abonné lorsqu'un événement, un article, une ressource ou un projet est publié.",
        "Vous êtes abonné à la lettre d'information de dotnetNiger.",
        [
            EmailVariables.TypeContenu,
            EmailVariables.TitreContenu,
            EmailVariables.ResumeContenu,
            EmailVariables.UrlContenu,
            EmailVariables.UrlDesabonnement,
        ],
        valeurs =>
            $"{valeurs.Texte(EmailVariables.TypeContenu)} sur dotnetNiger : {valeurs.Texte(EmailVariables.TitreContenu)}",
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
            (EmailVariables.UrlDesabonnement, "https://dotnetniger.test/lettre-information/desinscription?jeton=apercu")));

    private static readonly EmailTemplate InscriptionEvenement = new(
        EmailTemplateKeys.EventRegistrationConfirmed,
        "Inscription à un événement",
        "Envoyé lorsqu'une place est réservée.",
        "Vous vous êtes inscrit à un événement sur dotnetNiger.",
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
            (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet")));

    private static readonly EmailTemplate ListeAttente = new(
        EmailTemplateKeys.EventRegistrationWaitlisted,
        "Inscription en liste d'attente",
        "Envoyé lorsque l'événement est complet, pour signaler que la demande est retenue.",
        "Vous avez demandé une place pour un événement complet sur dotnetNiger.",
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
            (EmailVariables.UrlDesinscriptionEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet/inscriptions?jeton=apercu")));

    private static readonly EmailTemplate PlaceLiberee = new(
        EmailTemplateKeys.EventWaitlistPromoted,
        "Une place s'est libérée",
        "Envoyé à la personne suivante de la liste d'attente, pour lui laisser confirmer sa place.",
        "Vous êtes sur la liste d'attente d'un événement dotnetNiger.",
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
            (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet")));

    private static readonly EmailTemplate EvenementAnnule = new(
        EmailTemplateKeys.EventCancelled,
        "Événement annulé",
        "Envoyé à tous les inscrits lorsqu'un événement est annulé.",
        "Vous êtes inscrit à un événement sur dotnetNiger.",
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
            (EmailVariables.DateEvenement, "samedi 18 mars 2026 à 16 h")));

    private static readonly EmailTemplate EvenementModifie = new(
        EmailTemplateKeys.EventUpdated,
        "Événement modifié",
        "Envoyé aux inscrits lorsque la date, le lieu ou le programme change.",
        "Vous êtes inscrit à un événement sur dotnetNiger.",
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
            (EmailVariables.UrlEvenement, "https://dotnetniger.test/evenements/rencontre-dotnet")));

    private static readonly EmailTemplate Commentaire = new(
        EmailTemplateKeys.CommentPosted,
        "Nouveau commentaire",
        "Envoyé à l'auteur d'un contenu dès qu'un membre le commente.",
        "Vous publiez sur dotnetNiger.",
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
            (EmailVariables.ExtraitCommentaire, "Merci pour l'explication, le graphique m'a beaucoup aidé.")));

    private static readonly EmailTemplate RemiseEnLigne = new(
        EmailTemplateKeys.ContentRepublished,
        "Contenu remis en ligne",
        "Envoyé à l'auteur lorsqu'une de ses publications archivée redevient visible.",
        "Vous êtes l'auteur d'une publication sur dotnetNiger.",
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
            (EmailVariables.UrlContenu, "https://dotnetniger.test/ressources/deboguer-un-interblocage")));

    private static readonly EmailTemplate AccuseContact = new(
        EmailTemplateKeys.ContactAcknowledged,
        "Accusé de réception d'un message",
        "Envoyé à la personne qui a écrit via le formulaire de contact.",
        "Vous avez écrit à l'administration de dotnetNiger.",
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
            (EmailVariables.MessageContact, "Bonjour, je souhaite animer un atelier d'initiation à C# à l'université Abdou Moumouni.")));

    private static readonly EmailTemplate CopieContact = new(
        EmailTemplateKeys.ContactForwarded,
        "Nouveau message de contact",
        "Envoyé à l'administration, avec le message reçu et les coordonnées de son auteur.",
        "Un membre a utilisé le formulaire de contact de dotnetNiger.",
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
            (EmailVariables.MessageContact, "Bonjour, je souhaite animer un atelier d'initiation à C# à l'université Abdou Moumouni.")));

    /// <summary>
    /// Tous les modèles, dans l'ordre où l'administration les découvre.
    /// </summary>
    public static IReadOnlyList<EmailTemplate> Tous { get; } =
    [
        ConfirmationCompte,
        Bienvenue,
        Reinitialisation,
        SuppressionCompte,
        ConfirmationAbonnement,
        Desabonnement,
        NouveauContenu,
        InscriptionEvenement,
        ListeAttente,
        PlaceLiberee,
        EvenementAnnule,
        EvenementModifie,
        Commentaire,
        RemiseEnLigne,
        AccuseContact,
        CopieContact,
    ];

    /// <summary>
    /// Index des modèles par clé.
    /// </summary>
    private static FrozenDictionary<string, EmailTemplate> Index { get; } =
        FrozenDictionary.ToFrozenDictionary(
            Tous.ToDictionary(modele => modele.Cle, StringComparer.Ordinal));

    /// <summary>
    /// Toutes les clés déclarées, à seule fin de test.
    /// </summary>
    public static IReadOnlyCollection<string> Cles { get; } = Index.Keys;

    /// <summary>
    /// Récupère un modèle par sa clé.
    /// </summary>
    /// <param name="cle">Clé du modèle.</param>
    /// <param name="modele">Le modèle trouvé.</param>
    /// <returns>Vrai si le modèle existe.</returns>
    public static bool Essayer(string cle, out EmailTemplate? modele)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cle);

        return Index.TryGetValue(cle, out modele);
    }

    /// <summary>
    /// Récupère un modèle par sa clé, ou échoue.
    /// </summary>
    /// <param name="cle">Clé du modèle.</param>
    /// <returns>Le modèle.</returns>
    /// <exception cref="InvalidOperationException">La clé ne correspond à aucun modèle.</exception>
    public static EmailTemplate Obtenir(string cle) =>
        Essayer(cle, out var modele) && modele is not null
            ? modele
            : throw new InvalidOperationException(
                $"Aucun modèle de courriel pour la clé « {cle} ».");
}
