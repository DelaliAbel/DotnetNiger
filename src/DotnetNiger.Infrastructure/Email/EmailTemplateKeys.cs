namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Clés des modèles de courriel, telles qu'elles apparaissent dans la file d'attente.
/// </summary>
/// <remarks>
/// Une clé n'est jamais écrite par un appelant : elle est choisie par le code qui décide
/// qu'un message doit partir. Elle sert à regrouper les envois dans les diagnostics
/// (« combien de confirmations d'inscription ont échoué ? ») sans avoir à deviner le
/// contenu du message.
/// </remarks>
public static class EmailTemplateKeys
{
    /// <summary>Confirmation de l'adresse électronique d'un compte.</summary>
    public const string AccountConfirmation = "account.confirmation";

    /// <summary>Accueil d'un compte dont l'adresse est confirmée.</summary>
    public const string AccountWelcome = "account.welcome";

    /// <summary>Réinitialisation du mot de passe.</summary>
    public const string AccountPasswordReset = "account.password-reset";

    /// <summary>Rappel des deux confirmations exigées avant une suppression de compte.</summary>
    public const string AccountDeletionConfirmation = "account.deletion-confirmation";

    /// <summary>Confirmation d'un abonnement à la lettre d'information.</summary>
    public const string NewsletterConfirmation = "newsletter.confirmation";

    /// <summary>Accusé de désinscription, pour confirmer que la demande a été prise en compte.</summary>
    public const string NewsletterUnsubscribed = "newsletter.unsubscribed";

    /// <summary>Diffusion d'un contenu nouvellement publié.</summary>
    public const string NewsletterNewContent = "newsletter.new-content";

    /// <summary>Inscription à un événement confirmée.</summary>
    public const string EventRegistrationConfirmed = "event.registration-confirmed";

    /// <summary>Inscription placée en liste d'attente, faute de place disponible.</summary>
    public const string EventRegistrationWaitlisted = "event.registration-waitlisted";

    /// <summary>Une place s'est libérée et le membre est invité à la confirmer.</summary>
    public const string EventWaitlistPromoted = "event.waitlist-promoted";

    /// <summary>Un événement a été annulé ou retiré du site.</summary>
    public const string EventCancelled = "event.cancelled";

    /// <summary>Un événement a été modifié, déplacé ou mis à jour.</summary>
    public const string EventUpdated = "event.updated";

    /// <summary>Nouveau commentaire sur un contenu ou un événement.</summary>
    public const string CommentPosted = "comment.posted";

    /// <summary>Un contenu a été remis en ligne par l'administration.</summary>
    public const string ContentRepublished = "content.republished";

    /// <summary>Accusé de réception d'un message depuis le formulaire de contact.</summary>
    public const string ContactAcknowledged = "contact.acknowledged";

    /// <summary>Copie d'un message de contact adressée à l'administration.</summary>
    public const string ContactForwarded = "contact.forwarded";

    /// <summary>Réponse de l'administration au demandeur.</summary>
    public const string ContactReplied = "contact.replied";
}
