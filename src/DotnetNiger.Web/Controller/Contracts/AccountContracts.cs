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
public sealed record UpdateAccountRequest(
    bool IsActive,
    bool IsTeam);

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
public sealed record CreateAccountRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password,
    [StringLength(100)] string? FirstName = null,
    [StringLength(100)] string? LastName = null,
    bool IsTeam = false,
    bool ConfirmEmail = false,
    IReadOnlyList<string>? Roles = null);

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

