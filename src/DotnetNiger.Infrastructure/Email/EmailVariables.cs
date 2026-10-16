namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Noms des variables fournies aux modèles de courriel.
/// </summary>
/// <remarks>
/// Les modèles sont des lambdas : une faute de frappe sur un nom de variable ne serait
/// détectée qu'à l'envoi, donc en production, et seulement pour un membre. Les noms sont
/// donc regroupés ici, et les tests vérifient qu'un modèle se rend bien avec son exemple —
/// ce qui échoue dès qu'un nom manque ou n'est plus le bon.
/// </remarks>
public static class EmailVariables
{
    /// <summary>Nom affiché du membre destinataire.</summary>
    public const string NomMembre = "NomMembre";

    /// <summary>Adresse du site à propos de la suppression du compte.</summary>
    public const string UrlCompte = "UrlCompte";

    /// <summary>Lien de confirmation de l'adresse.</summary>
    public const string UrlConfirmation = "UrlConfirmation";

    /// <summary>Lien de choix d'un nouveau mot de passe.</summary>
    public const string UrlReinitialisation = "UrlReinitialisation";

    /// <summary>Lien de confirmation de la suppression du compte.</summary>
    public const string UrlConfirmationSuppression = "UrlConfirmationSuppression";

    /// <summary>Date d'échéance de la suppression, en texte.</summary>
    public const string DelaiSuppression = "DelaiSuppression";

    /// <summary>Lien de désabonnement propre au destinataire.</summary>
    public const string UrlDesabonnement = "UrlDesabonnement";

    /// <summary>Intitulé de l'événement.</summary>
    public const string TitreEvenement = "TitreEvenement";

    /// <summary>Date et heure de l'événement, en texte lisible.</summary>
    public const string DateEvenement = "DateEvenement";

    /// <summary>Lieu de l'événement.</summary>
    public const string LieuEvenement = "LieuEvenement";

    /// <summary>Page de l'événement.</summary>
    public const string UrlEvenement = "UrlEvenement";

    /// <summary>Page de désinscription à l'événement.</summary>
    public const string UrlDesinscriptionEvenement = "UrlDesinscriptionEvenement";

    /// <summary>Nature du contenu publié : événement, article, ressource ou projet.</summary>
    public const string TypeContenu = "TypeContenu";

    /// <summary>Intitulé du contenu.</summary>
    public const string TitreContenu = "TitreContenu";

    /// <summary>Résumé du contenu.</summary>
    public const string ResumeContenu = "ResumeContenu";

    /// <summary>Page du contenu.</summary>
    public const string UrlContenu = "UrlContenu";

    /// <summary>Nom du membre qui a publié le contenu.</summary>
    public const string AuteurContenu = "AuteurContenu";

    /// <summary>Nom du membre à l'origine du commentaire.</summary>
    public const string AuteurCommentaire = "AuteurCommentaire";

    /// <summary>Extrait du commentaire.</summary>
    public const string ExtraitCommentaire = "ExtraitCommentaire";

    /// <summary>Nom de la personne qui écrit à l'administration.</summary>
    public const string NomContact = "NomContact";

    /// <summary>Adresse de la personne qui écrit à l'administration.</summary>
    public const string EmailContact = "EmailContact";

    /// <summary>Objet du message reçu par le formulaire de contact.</summary>
    public const string SujetContact = "SujetContact";

    /// <summary>Corps du message reçu par le formulaire de contact.</summary>
    public const string MessageContact = "MessageContact";

    /// <summary>Réponse rédigée par l'administration au demandeur.</summary>
    public const string MessageReponse = "MessageReponse";
}
