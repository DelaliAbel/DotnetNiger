namespace DotnetNiger.Community.Application.Constants;

/// <summary>Messages statiques utilisés dans les réponses API (succès, erreurs, notifications).</summary>
public static class Messages
{
    public static class Certificate
    {
        public const string NeedValidCertificate = "Vous devez avoir un certificat validé pour créer du contenu.";
        public const string InvalidUrl = "URL de certification invalide.";
        public const string TypeRequired = "Veuillez sélectionner un type de certificat.";
        public const string NotFound = "Certificat introuvable.";
        public const string RejectReasonRequired = "La raison du rejet est requise.";
        public const string EstimatedWait = "24-48 heures";
        public const string SupportEmail = "support@dotnetniger.org";
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusRejected = "Rejected";
    }

    public static class User
    {
        public const string InvalidIdentity = "Identifiant utilisateur invalide.";
        public const string NotFound = "Utilisateur introuvable.";
        public const string StatusUpdated = "Statut de l'utilisateur mis à jour.";
        public const string TeamUpdated = "Statut d'équipe mis à jour.";
        public const string Created = "Utilisateur créé avec succès.";
        public const string CreateFailed = "Échec de la création de l'utilisateur";
        public const string Deleted = "Utilisateur supprimé.";
        public const string PromoteFailed = "Échec de la promotion de l'utilisateur.";
        public const string Promoted = "Utilisateur promu Admin avec succès.";
        public const string RoleAssigned = "Rôle assigné avec succès.";
        public const string RoleFailed = "Échec de l'assignation du rôle.";
        public const string PermissionAssigned = "Permission assignée avec succès.";
        public const string PermissionFailed = "Échec de l'assignation de la permission.";
    }

    public static class Post
    {
        public const string NotFound = "Publication introuvable.";
        public const string Deleted = "Publication supprimée.";
        public const string NotAuthorizedModify = "Vous n'êtes pas autorisé à modifier cette publication.";
        public const string NotAuthorizedPublish = "Vous n'êtes pas autorisé à publier cette publication.";
        public const string NotAuthorizedUnpublish = "Vous n'êtes pas autorisé à dépublier cette publication.";
        public const string NotAuthorizedDelete = "Vous n'êtes pas autorisé à supprimer cette publication.";
        public const string NotAuthorizedCreate = "Vous n'êtes pas autorisé à créer une publication.";
    }

    public static class Event
    {
        public const string NotFound = "Événement introuvable.";
        public const string Deleted = "Événement supprimé.";
        public const string Rejected = "Événement rejeté.";
        public const string FullOrRegistered = "L'événement est complet ou vous êtes déjà inscrit.";
        public const string RegistrationNotFound = "Inscription introuvable.";
        public const string RegistrationCancelled = "Inscription annulée.";
        public const string NotAuthorizedModify = "Vous n'êtes pas autorisé à modifier cet événement.";
        public const string NotAuthorizedDelete = "Vous n'êtes pas autorisé à supprimer cet événement.";
    }

    public static class Resource
    {
        public const string NotFound = "Ressource introuvable.";
        public const string Deleted = "Ressource supprimée.";
        public const string NotAuthorizedModify = "Vous n'êtes pas autorisé à modifier cette ressource.";
        public const string NotAuthorizedDelete = "Vous n'êtes pas autorisé à supprimer cette ressource.";
    }

    public static class Project
    {
        public const string NotFound = "Projet non trouvé.";
        public const string Deleted = "Projet supprimé.";
        public const string NotAuthorizedModify = "Vous n'êtes pas autorisé à modifier ce projet.";
        public const string NotAuthorizedDelete = "Vous n'êtes pas autorisé à supprimer ce projet.";
    }

    public static class Comment
    {
        public const string NotFound = "Commentaire introuvable.";
        public const string Deleted = "Commentaire supprimé.";
        public const string DeletedPlaceholder = "[Supprimé]";
    }

    public static class Category
    {
        public const string NotFound = "Catégorie introuvable.";
        public const string Deleted = "Catégorie supprimée.";
    }

    public static class Tag
    {
        public const string NotFound = "Tag introuvable.";
        public const string Deleted = "Tag supprimé.";
    }

    public static class Partner
    {
        public const string NotFound = "Partenaire non trouvé.";
        public const string Deleted = "Partenaire supprimé.";
    }

    public static class Member
    {
        public const string NotFound = "Membre non trouvé.";
    }

    public static class Profile
    {
        public const string NotFound = "Profil introuvable.";
        public const string SocialLinkNotFound = "Lien social introuvable.";
        public const string SocialLinkDeleted = "Lien social supprimé.";
    }

    public static class Newsletter
    {
        public const string AlreadySubscribed = "Cet email est déjà abonné.";
        public const string NotFoundOrUnsubscribed = "Abonnement non trouvé ou déjà désabonné.";
        public const string Unsubscribed = "Désabonnement réussi.";
    }

    public static class Notification
    {
        public const string MessageRequired = "Le message est requis.";
        public const string Sent = "Notification envoyée.";
        public const string NotFound = "Notification introuvable.";
        public const string MarkedAsRead = "Notification marquée comme lue.";
        public const string AllMarkedAsRead = "Toutes les notifications marquées comme lues.";
    }

    public static class Upload
    {
        public const string NoFile = "Aucun fichier fourni.";
        public const string ExtensionNotAllowed = "Extension non autorisée : ";
        public const string MimeNotAllowed = "Type MIME non autorisé : ";
        public const string TooLarge = "Fichier trop volumineux (max 3 Mo).";
        public const string Uploaded = "Image uploadée avec succès.";
        public const string PathRequired = "Chemin requis.";
        public const string NotFound = "Fichier introuvable.";
        public const string Deleted = "Fichier supprimé.";
        public const string InvalidImage = "Format d'image non valide ou corrompu.";
        public const string TypeNotAllowed = "Type d'image non autorisé.";
    }

    public static class Contact
    {
        public const string AllFieldsRequired = "Tous les champs sont requis.";
        public const string Sent = "Message envoyé avec succès.";
        public const string Error = "Erreur lors de l'envoi.";
    }

    public static class Error
    {
        public const string InternalError = "Une erreur interne est survenue.";
        public const string NotFound = "Ressource non trouvée.";
        public const string BadRequest = "Requête invalide.";
        public const string Forbidden = "Accès refusé.";
    }

    public static class Content
    {
        public const string NotAuthorized = "Vous n'avez pas les permissions nécessaires.";
    }

    public static class Validation
    {
        public const string TitleRequired = "Le titre est requis.";
        public const string TitleTooLong = "Le titre est trop long.";
        public const string ContentRequired = "Le contenu est requis.";
    }
}
