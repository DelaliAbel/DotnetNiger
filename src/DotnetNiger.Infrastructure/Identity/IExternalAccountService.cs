using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Issue du traitement d'un retour de connexion externe.
/// </summary>
public enum ExternalSignInStatus
{
    /// <summary>
    /// L'identité externe était déjà rattachée : connexion directe.
    /// </summary>
    Connected = 0,

    /// <summary>
    /// L'adresse, certifiée par le fournisseur, correspondait à un compte existant. Le
    /// compte a été rattaché puis la session ouverte, sans ressaisie du mot de passe.
    /// </summary>
    LinkedToExistingAccount = 1,

    /// <summary>
    /// Identité inconnue : aucun compte n'est créé tant que le membre n'a pas choisi de mot
    /// de passe. Un compte sans mot de passe ne serait authentifiable que par ce
    /// fournisseur, et la saisie ultérieure buterait sur un doublon d'adresse.
    /// </summary>
    PasswordRequired = 2,

    /// <summary>
    /// Un compte existe déjà pour cette adresse, mais le fournisseur ne l'a pas certifiée.
    /// Le rattachement est refusé : seule la saisie du mot de passe existant peut prouver
    /// que la personne est bien la titulaire du compte.
    /// </summary>
    PasswordProofRequired = 3,
}

/// <summary>
/// Résultat détaillé d'un retour de connexion externe.
/// </summary>
/// <param name="Status">Issue du traitement.</param>
/// <param name="User">Compte concerné lorsqu'il est connu.</param>
/// <param name="Email">Adresse transmise par le fournisseur.</param>
/// <param name="EmailVerified">Vrai si le fournisseur a certifié l'adresse.</param>
public sealed record ExternalSignInResult(
    ExternalSignInStatus Status,
    ApplicationUser? User,
    string? Email,
    bool EmailVerified)
{
    /// <summary>
    /// Vrai si la session peut être ouverte immédiatement.
    /// </summary>
    public bool CanSignIn =>
        Status is ExternalSignInStatus.Connected or ExternalSignInStatus.LinkedToExistingAccount;
}

/// <summary>
/// Traitement des retours de connexion depuis un fournisseur externe.
/// </summary>
public interface IExternalAccountService
{
    /// <summary>
    /// Reprend la session après une authentification réussie chez un fournisseur externe.
    /// </summary>
    /// <param name="provider">Fournisseur, par exemple <c>Google</c>.</param>
    /// <param name="providerKey">Identifiant stable du compte chez ce fournisseur.</param>
    /// <param name="email">Adresse transmise par le fournisseur.</param>
    /// <param name="claims">Revendications transmises par le fournisseur.</param>
    /// <returns>Ce qu'il reste à faire pour ouvrir une session.</returns>
    Task<ExternalSignInResult> Complete(
        string provider,
        string providerKey,
        string? email,
        IEnumerable<Claim> claims);

    /// <summary>
    /// Crée le compte d'un membre arrivé par un fournisseur, et rattache l'identité.
    /// </summary>
    /// <param name="user">Compte à enregistrer.</param>
    /// <param name="password">Mot de passe choisi par le membre.</param>
    /// <param name="provider">Fournisseur d'origine.</param>
    /// <param name="providerKey">Identifiant du compte chez ce fournisseur.</param>
    /// <param name="providerDisplayName">Nom lisible du fournisseur.</param>
    /// <returns>Résultat de l'opération Identity.</returns>
    Task<IdentityResult> CreateWithPasswordAsync(
        ApplicationUser user,
        string password,
        string provider,
        string providerKey,
        string? providerDisplayName);

    /// <summary>
    /// Rattache un fournisseur externe au compte connecté, après vérification du mot de passe.
    /// </summary>
    /// <param name="provider">Fournisseur à rattacher.</param>
    /// <param name="providerKey">Identifiant du compte chez ce fournisseur.</param>
    /// <param name="providerDisplayName">Nom lisible du fournisseur.</param>
    /// <returns>Résultat de l'opération Identity.</returns>
    Task<IdentityResult> LinkAsync(string provider, string providerKey, string? providerDisplayName);
}
