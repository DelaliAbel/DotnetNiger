using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Compte tel que l'administration le consulte.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Email">Adresse électronique.</param>
/// <param name="Slug">Slug du profil.</param>
/// <param name="FullName">Nom complet.</param>
/// <param name="IsActive">Compte actif, ou désactivé par l'administration.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
/// <param name="EmailConfirmed">Adresse électronique confirmée.</param>
/// <param name="Roles">Rôles attribués.</param>
/// <param name="LockoutEnd">Fin de verrouillage, après trop de tentatives.</param>
/// <param name="CreatedAtUtc">Date d'inscription.</param>
public sealed record AdminAccountResponse(
    Guid Id,
    string? Email,
    string? Slug,
    string FullName,
    bool IsActive,
    bool IsTeam,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    DateTimeOffset? LockoutEnd,
    DateTime CreatedAtUtc);

/// <summary>Demande de modération d'un compte.</summary>
/// <param name="IsActive">Vrai pour réactiver, faux pour désactiver.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
/// <param name="FirstName">Prénom, facultatif.</param>
/// <param name="LastName">Nom, facultatif.</param>
/// <param name="Profession">Fonction affichée sur la page « À propos », facultative.</param>
/// <param name="PhotoUrl">Adresse de la photo, facultative.</param>
/// <param name="GitHubUsername">Identifiant GitHub, avec ou sans « @ ».</param>
/// <param name="LinkedInUrl">Adresse du profil LinkedIn.</param>
/// <param name="XUrl">Adresse du compte X.</param>
/// <remarks>
/// Les champs de profil sont tous facultatifs et tous remplaçants : omettre un champ le
/// laisse inchangé. Les effacer — pour retirer une photo devenue fausse, par exemple — se
/// fait en envoyant une chaîne vide, que le modèle ramène à <see langword="null"/>.
/// </remarks>
public sealed record UpdateAccountRequest(
    bool IsActive,
    bool IsTeam,
    string? FirstName = null,
    string? LastName = null,
    string? Profession = null,
    string? PhotoUrl = null,
    string? GitHubUsername = null,
    string? LinkedInUrl = null,
    string? XUrl = null);

/// <summary>Rôles à attribuer à un compte.</summary>
/// <param name="Roles">Rôles complets attendus après l'opération.</param>
public sealed record ChangeRolesRequest(
    [Required, MinLength(1)] IReadOnlyList<string> Roles);

/// <summary>Création d'un compte par l'administration.</summary>
/// <param name="Email">Adresse de connexion.</param>
/// <param name="Password">Mot de passe initial, soumis à la politique du site.</param>
/// <param name="FirstName">Prénom, facultatif.</param>
/// <param name="LastName">Nom, facultatif.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
/// <param name="ConfirmEmail">
/// Vrai si l'administration atteste que l'adresse a été contrôlée par un autre canal.
/// À faux, le membre devra confirmer son adresse avant de se connecter.
/// </param>
/// <param name="Roles">Rôles attribués. <c>Member</c> si la liste est omise.</param>
/// <param name="Profession">Fonction affichée sur la page « À propos », facultative.</param>
/// <param name="PhotoUrl">
/// Adresse de la photo, facultative. Accepte aussi un chemin servi par le site, de la
/// forme <c>/api/images/…</c>, ce qui évite d'y écrire le nom de domaine.
/// </param>
/// <param name="GitHubUsername">Identifiant GitHub, avec ou sans « @ ».</param>
/// <param name="LinkedInUrl">Adresse du profil LinkedIn.</param>
/// <param name="XUrl">Adresse du compte X.</param>
/// <remarks>
/// Les cinq derniers champs ne servent qu'à la carte d'équipe de la page « À propos ».
/// Ils sont optionnels parce qu'un compte de test n'a pas de photographie à fournir, et que
/// les rendre obligatoires ferait échouer des créations parfaitement légitimes.
/// </remarks>
public sealed record CreateAccountRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password,
    [StringLength(100)] string? FirstName = null,
    [StringLength(100)] string? LastName = null,
    bool IsTeam = false,
    bool ConfirmEmail = false,
    IReadOnlyList<string>? Roles = null,
    [StringLength(200)] string? Profession = null,
    [StringLength(1000)] string? PhotoUrl = null,
    [StringLength(100)] string? GitHubUsername = null,
    [StringLength(500)] string? LinkedInUrl = null,
    [StringLength(500)] string? XUrl = null);

/// <summary>Suppression définitive du compte du membre connecté.</summary>
/// <param name="ConfirmEmail">Adresse du compte, à recopier pour confirmer.</param>
/// <param name="ConfirmPermanentDeletion">Vrai pour confirmer l'irréversibilité.</param>
/// <remarks>
/// Deux confirmations sont exigées parce que l'opération est irréversible : l'interface
/// les présente l'une après l'autre, et le serveur refuse une demande qui n'en porte
/// qu'une, afin qu'un appel isolé — un lien suivi par erreur, un script — ne puisse pas
/// effacer un compte.
/// </remarks>
public sealed record DeleteOwnAccountRequest(
    [Required, EmailAddress, StringLength(256)] string ConfirmEmail,
    bool ConfirmPermanentDeletion = false);

