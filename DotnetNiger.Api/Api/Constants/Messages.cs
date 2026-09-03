namespace DotnetNiger.Api.Constants;

/// <summary>
/// Messages d'erreur standardisés de l'application.
/// </summary>
public static class ErrorMessages
{
    public const string UserNotFound = "Utilisateur non trouvé";
    public const string UserAlreadyExists = "Un utilisateur avec cet email existe déjà";
    public const string UnableToAssignRole = "Impossible d'assigner le rôle";
    public const string RoleNotFound = "Rôle introuvable";
    public const string InvalidCredentials = "Email ou mot de passe incorrect.";
    public const string AccountLocked = "Compte temporairement verrouillé. Réessayez plus tard.";
    public const string InternalError = "Une erreur interne est survenue.";
    public const string ResourceNotFound = "Ressource non trouvée.";
    public const string BadRequest = "Requête invalide.";
    public const string Forbidden = "Accès refusé.";
    public const string AccessDenied = "Vous n'avez pas les permissions nécessaires.";
}

/// <summary>
/// Messages de succès standardisés de l'application.
/// </summary>
public static class SuccessMessages
{
    /// <summary>Message d'invitation envoyée.</summary>
    public const string InvitationSent = "Invitation envoyée avec succès.";
    public const string StatusUpdated = "Statut mis à jour avec succès";
    public const string RoleAssigned = "Rôle assigné avec succès";
    public const string RoleRemoved = "Rôle retiré avec succès";
    public const string UserDeleted = "Utilisateur supprimé avec succès";
}

/// <summary>
/// Messages de réponse standardisés par domaine métier.
/// </summary>
public static class Messages
{
    /// <summary>Messages liés aux utilisateurs.</summary>
    public static class User
    {
        public const string NotFound = "Utilisateur non trouvé";
        public const string InvalidIdentity = "Identité invalide";
        public const string CreateFailed = "Impossible de créer l'utilisateur";
        public const string Deleted = "Utilisateur supprimé avec succès";
        public const string StatusUpdated = "Statut mis à jour";
        public const string TeamUpdated = "Équipe mise à jour";
        public const string RoleAssigned = "Rôle assigné avec succès";
        public const string RoleRemoved = "Rôle retiré avec succès";
        public const string RoleFailed = "Opération sur le rôle échouée";
        public const string Promoted = "Utilisateur promu admin";
        public const string PromoteFailed = "Impossible de promouvoir l'utilisateur";
    }

    /// <summary>Messages liés aux catégories.</summary>
    public static class Category
    {
        public const string NotFound = "Catégorie non trouvée";
        public const string Deleted = "Catégorie supprimée avec succès";
    }

    /// <summary>Messages liés aux événements.</summary>
    public static class Event
    {
        public const string NotFound = "Événement non trouvé";
        public const string Deleted = "Événement supprimé avec succès";
        public const string Rejected = "Événement rejeté";
        public const string FullOrRegistered = "Événement complet ou déjà inscrit";
        public const string RegistrationNotFound = "Inscription non trouvée";
        public const string RegistrationCancelled = "Inscription annulée";
        public const string RegistrationsForbidden = "Vous n'avez pas accès aux inscriptions de cet événement";
    }

    /// <summary>Messages liés aux articles.</summary>
    public static class Post
    {
        public const string NotFound = "Article non trouvé";
        public const string Deleted = "Article supprimé avec succès";
    }

    /// <summary>Messages liés aux ressources.</summary>
    public static class Resource
    {
        public const string NotFound = "Ressource non trouvée";
        public const string Deleted = "Ressource supprimée avec succès";
    }

    /// <summary>Messages liés aux projets.</summary>
    public static class Project
    {
        public const string NotFound = "Projet non trouvé";
        public const string Deleted = "Projet supprimé avec succès";
    }

    /// <summary>Messages liés aux commentaires.</summary>
    public static class Comment
    {
        public const string NotFound = "Commentaire non trouvé";
        public const string Deleted = "Commentaire supprimé avec succès";
        public const string Reported = "Commentaire signalé.";
    }

    /// <summary>Messages liés aux tags.</summary>
    public static class Tag
    {
        public const string NotFound = "Tag non trouvé";
        public const string Deleted = "Tag supprimé avec succès";
    }

    /// <summary>Messages liés aux partenaires.</summary>
    public static class Partner
    {
        public const string NotFound = "Partenaire non trouvé";
        public const string Deleted = "Partenaire supprimé avec succès";
    }

    /// <summary>Messages liés aux membres.</summary>
    public static class Member
    {
        public const string NotFound = "Membre non trouvé";
    }

    /// <summary>Messages liés aux certificats.</summary>
    public static class Certificate
    {
        public const string NotFound = "Certificat non trouvé";
        public const string RejectReasonRequired = "Un motif de rejet est requis";
    }

    /// <summary>Messages liés à la newsletter.</summary>
    public static class Newsletter
    {
        public const string NotFoundOrUnsubscribed = "Email non trouvé ou déjà désabonné";
        public const string Unsubscribed = "Désabonnement effectué avec succès";
        public const string Deleted = "Abonnement supprimé avec succès";
        public const string Confirmed = "Inscription confirmée avec succès";
        public const string InvalidOrExpiredToken = "Token de confirmation invalide ou expiré";
        public const string Sent = "Newsletter envoyée avec succès";
        public const string NoRecipients = "Aucun abonné actif et confirmé à notifier";
    }

    /// <summary>Messages liés aux notifications.</summary>
    public static class Notification
    {
        public const string NotFound = "Notification non trouvée";
        public const string MessageRequired = "Le message est requis";
        public const string Sent = "Notification envoyée";
        public const string MarkedAsRead = "Notification marquée comme lue";
        public const string AllMarkedAsRead = "Toutes les notifications marquées comme lues";
    }

    /// <summary>Messages liés aux paramètres.</summary>
    public static class Setting
    {
        public const string NotFound = "Paramètre non trouvé";
        public const string Updated = "Paramètre mis à jour";
        public const string BatchUpdated = "Paramètres mis à jour";
        public const string Deleted = "Paramètre supprimé";
    }

    /// <summary>Messages liés au formulaire de contact.</summary>
    public static class Contact
    {
        public const string AllFieldsRequired = "Tous les champs sont requis";
        public const string Sent = "Message envoyé avec succès";
        public const string Error = "Erreur lors de l'envoi du message";
    }

    /// <summary>Messages liés aux uploads de fichiers.</summary>
    public static class Upload
    {
        public const string NoFile = "Aucun fichier fourni";
        public const string Uploaded = "Fichier téléchargé avec succès";
        public const string Deleted = "Fichier supprimé avec succès";
        public const string NotFound = "Fichier non trouvé";
        public const string InvalidImage = "Image invalide";
        public const string TooLarge = "Fichier trop volumineux";
        public const string PathRequired = "Le chemin est requis";
    }

    /// <summary>Messages liés au profil utilisateur.</summary>
    public static class Profile
    {
        public const string NotFound = "Profil non trouvé";
        public const string SocialLinkNotFound = "Lien social non trouvé";
        public const string SocialLinkDeleted = "Lien social supprimé";
    }
}
