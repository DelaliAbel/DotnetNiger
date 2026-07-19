namespace DotnetNiger.Domain.Constants;

public static class ErrorMessages
{
    public const string UserNotFound = "Utilisateur non trouvé";
    public const string UserAlreadyExists = "Un utilisateur avec cet email existe déjà";
    public const string UnableToAssignRole = "Impossible d'assigner le rôle";
    public const string RoleNotFound = "Rôle introuvable";
    public const string InvalidCredentials = "Email ou mot de passe incorrect.";
    public const string AccountLocked = "Compte temporairement verrouillé. Réessayez plus tard.";
    public const string TwoFactorRequired = "Authentification à deux facteurs requise (non configurée).";
    public const string InternalError = "Une erreur interne est survenue.";
    public const string ResourceNotFound = "Ressource non trouvée.";
    public const string BadRequest = "Requête invalide.";
    public const string Forbidden = "Accès refusé.";
    public const string AccessDenied = "Vous n'avez pas les permissions nécessaires.";
}

public static class SuccessMessages
{
    public const string InvitationSent = "Invitation envoyée avec succès.";
    public const string StatusUpdated = "Statut mis à jour avec succès";
    public const string RoleAssigned = "Rôle assigné avec succès";
    public const string RoleRemoved = "Rôle retiré avec succès";
    public const string UserDeleted = "Utilisateur supprimé avec succès";
}

public static class Messages
{
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

    public static class Category
    {
        public const string NotFound = "Catégorie non trouvée";
        public const string Deleted = "Catégorie supprimée avec succès";
    }

    public static class Event
    {
        public const string NotFound = "Événement non trouvé";
        public const string Deleted = "Événement supprimé avec succès";
        public const string Rejected = "Événement rejeté";
        public const string FullOrRegistered = "Événement complet ou déjà inscrit";
        public const string RegistrationNotFound = "Inscription non trouvée";
        public const string RegistrationCancelled = "Inscription annulée";
    }

    public static class Post
    {
        public const string NotFound = "Article non trouvé";
        public const string Deleted = "Article supprimé avec succès";
    }

    public static class Resource
    {
        public const string NotFound = "Ressource non trouvée";
        public const string Deleted = "Ressource supprimée avec succès";
    }

    public static class Project
    {
        public const string NotFound = "Projet non trouvé";
        public const string Deleted = "Projet supprimé avec succès";
    }

    public static class Comment
    {
        public const string NotFound = "Commentaire non trouvé";
        public const string Deleted = "Commentaire supprimé avec succès";
    }

    public static class Tag
    {
        public const string NotFound = "Tag non trouvé";
        public const string Deleted = "Tag supprimé avec succès";
    }

    public static class Partner
    {
        public const string NotFound = "Partenaire non trouvé";
        public const string Deleted = "Partenaire supprimé avec succès";
    }

    public static class Member
    {
        public const string NotFound = "Membre non trouvé";
    }

    public static class Certificate
    {
        public const string NotFound = "Certificat non trouvé";
        public const string RejectReasonRequired = "Un motif de rejet est requis";
    }

    public static class Newsletter
    {
        public const string NotFoundOrUnsubscribed = "Email non trouvé ou déjà désabonné";
        public const string Unsubscribed = "Désabonnement effectué avec succès";
    }

    public static class Notification
    {
        public const string NotFound = "Notification non trouvée";
        public const string MessageRequired = "Le message est requis";
        public const string Sent = "Notification envoyée";
        public const string MarkedAsRead = "Notification marquée comme lue";
        public const string AllMarkedAsRead = "Toutes les notifications marquées comme lues";
    }

    public static class Setting
    {
        public const string NotFound = "Paramètre non trouvé";
        public const string Updated = "Paramètre mis à jour";
        public const string BatchUpdated = "Paramètres mis à jour";
        public const string Deleted = "Paramètre supprimé";
    }

    public static class Contact
    {
        public const string AllFieldsRequired = "Tous les champs sont requis";
        public const string Sent = "Message envoyé avec succès";
        public const string Error = "Erreur lors de l'envoi du message";
    }

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

    public static class Profile
    {
        public const string NotFound = "Profil non trouvé";
        public const string SocialLinkNotFound = "Lien social non trouvé";
        public const string SocialLinkDeleted = "Lien social supprimé";
    }
}
