namespace DotnetNiger.Common.Constants;

/// <summary>Messages d'erreur centralisés.</summary>
public static class ErrorMessages
{
    /// <summary>Utilisateur introuvable.</summary>
    public const string UserNotFound = "Utilisateur non trouvé";

    /// <summary>Aucun tenant configuré dans le système.</summary>
    public const string TenantNotFound = "Aucun tenant trouvé";

    /// <summary>Email déjà utilisé par un autre compte.</summary>
    public const string UserAlreadyExists = "Un utilisateur avec cet email existe déjà";

    /// <summary>Échec d'assignation de rôle.</summary>
    public const string UnableToAssignRole = "Impossible d'assigner le rôle";

    /// <summary>Rôle introuvable dans le système.</summary>
    public const string RoleNotFound = "Rôle introuvable";

    /// <summary>Utilisateur non trouvé dans ce tenant.</summary>
    public const string UserNotInTenant = "Utilisateur non trouvé dans ce tenant";

    /// <summary>Identifiants de connexion invalides.</summary>
    public const string InvalidCredentials = "Email ou mot de passe incorrect.";

    /// <summary>Compte verrouillé pour cause de tentatives échouées.</summary>
    public const string AccountLocked = "Compte temporairement verrouillé. Réessayez plus tard.";

    /// <summary>2FA requise mais non configurée.</summary>
    public const string TwoFactorRequired = "Authentification à deux facteurs requise (non configurée).";

    /// <summary>Erreur interne du serveur.</summary>
    public const string InternalError = "Une erreur interne est survenue.";

    /// <summary>Ressource non trouvée.</summary>
    public const string ResourceNotFound = "Ressource non trouvée.";

    /// <summary>Requête invalide.</summary>
    public const string BadRequest = "Requête invalide.";

    /// <summary>Accès refusé.</summary>
    public const string Forbidden = "Accès refusé.";

    /// <summary>Permissions insuffisantes.</summary>
    public const string AccessDenied = "Vous n'avez pas les permissions nécessaires.";
}

/// <summary>Messages de succès centralisés.</summary>
public static class SuccessMessages
{
    /// <summary>Invitation admin envoyée par email.</summary>
    public const string InvitationSent = "Invitation envoyée avec succès.";

    /// <summary>Statut utilisateur mis à jour.</summary>
    public const string StatusUpdated = "Statut mis à jour avec succès";

    /// <summary>Rôle assigné avec succès.</summary>
    public const string RoleAssigned = "Rôle assigné avec succès";

    /// <summary>Utilisateur supprimé avec succès.</summary>
    public const string UserDeleted = "Utilisateur supprimé avec succès";
}
