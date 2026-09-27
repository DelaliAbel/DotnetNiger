using System.Security.Claims;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Traitement du retour d'un fournisseur d'identité externe.
/// </summary>
public sealed class ExternalCallbackHandler(
    SignInManager<ApplicationUser> signInManager,
    IExternalAccountService externalAccounts)
{
    private static readonly string ExternalCookieScheme = IdentityConstants.ExternalScheme;

    /// <summary>
    /// Reprend la session après une authentification réussie chez le fournisseur.
    /// </summary>
    /// <param name="httpContext">Requête courante.</param>
    /// <param name="provider">Fournisseur qui a authentifié le membre.</param>
    /// <param name="principal">Identité temporaire renvoyée par le fournisseur.</param>
    /// <param name="returnUrl">Destination interne demandée avant le départ.</param>
    /// <returns>Résultat de l'ouverture de session, ou de la demande de mot de passe.</returns>
    public async Task<ExternalCallbackOutcome> HandleAsync(
        HttpContext httpContext,
        string provider,
        ClaimsPrincipal principal,
        string? returnUrl)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(principal);

        var providerKey = ReadProviderKey(principal, provider);
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var result = await externalAccounts.Complete(provider, providerKey, email, principal.Claims);

        // La session temporaire contient aussi les informations de retour du fournisseur :
        // elle est supprimée dans tous les cas, qu'une session locale soit ouverte ou non.
        await httpContext.SignOutAsync(ExternalCookieScheme);

        switch (result.Status)
        {
            case ExternalSignInStatus.Connected:
            case ExternalSignInStatus.LinkedToExistingAccount:
                await signInManager.SignInAsync(result.User!, isPersistent: false);
                return ExternalCallbackOutcome.SignedIn(
                    ExternalSignInNavigation.IsLocal(returnUrl) ? returnUrl! : "/");

            case ExternalSignInStatus.PasswordProofRequired:
                return ExternalCallbackOutcome.PasswordRequired(
                    provider,
                    providerKey,
                    email,
                    emailVerified: result.EmailVerified,
                    requiresExistingPassword: true);

            case ExternalSignInStatus.PasswordRequired:
            default:
                // Aucun mot de passe n'a encore été choisi. La session n'est pas ouverte :
                // elle le sera par le point de terminaison qui enregistre le mot de passe, ce
                // qui garantit qu'aucun compte n'existe sans mot de passe sur le site.
                return ExternalCallbackOutcome.PasswordRequired(
                    provider,
                    providerKey,
                    email,
                    emailVerified: result.EmailVerified,
                    requiresExistingPassword: false);
        }
    }

    private static string ReadProviderKey(ClaimsPrincipal principal, string provider)
    {
        var key = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(key))
        {
            // Sans identifiant stable, aucune liaison n'est possible. L'échec est explicite
            // plutôt que silencieux : rattacher une clé vide regrouperait tous les comptes
            // d'un même fournisseur.
            throw new ExternalIdentityException(
                $"Le fournisseur {provider} n'a pas fourni d'identifiant de compte exploitable.");
        }

        return key;
    }
}

/// <summary>
/// Exception levée lorsque le retour d'un fournisseur est inexploitable.
/// </summary>
public sealed class ExternalIdentityException(string message) : InvalidOperationException(message);

/// <summary>
/// Ce qu'il reste à faire après le retour d'un fournisseur externe.
/// </summary>
/// <param name="Status">Issue du traitement.</param>
/// <param name="RedirectUrl">Destination quand la session est ouverte.</param>
/// <param name="Provider">Fournisseur d'origine.</param>
/// <param name="ProviderKey">Identité du compte chez le fournisseur.</param>
/// <param name="Email">Adresse transmise par le fournisseur.</param>
/// <param name="EmailVerified">Vrai si le fournisseur a certifié l'adresse.</param>
/// <param name="RequiresExistingPassword">Vrai si un compte existe déjà et doit être prouvé.</param>
public sealed record ExternalCallbackOutcome(
    ExternalCallbackStatus Status,
    string? RedirectUrl,
    string? Provider,
    string? ProviderKey,
    string? Email,
    bool EmailVerified,
    bool RequiresExistingPassword)
{
    /// <summary>
    /// Crée une issue de session ouverte.
    /// </summary>
    /// <param name="redirectUrl">Destination.</param>
    /// <returns>Issue correspondante.</returns>
    public static ExternalCallbackOutcome SignedIn(string redirectUrl) =>
        new(ExternalCallbackStatus.SignedIn, redirectUrl, null, null, null, false, false);

    /// <summary>
    /// Crée une issue demandant un mot de passe.
    /// </summary>
    /// <param name="provider">Fournisseur d'origine.</param>
    /// <param name="providerKey">Identité du compte chez le fournisseur.</param>
    /// <param name="email">Adresse transmise par le fournisseur.</param>
    /// <param name="emailVerified">Vrai si le fournisseur a certifié l'adresse.</param>
    /// <param name="requiresExistingPassword">Vrai si un compte doit être prouvé.</param>
    /// <returns>Issue correspondante.</returns>
    public static ExternalCallbackOutcome PasswordRequired(
        string provider,
        string providerKey,
        string? email,
        bool emailVerified,
        bool requiresExistingPassword) =>
        new(
            ExternalCallbackStatus.PasswordRequired,
            null,
            provider,
            providerKey,
            email,
            emailVerified,
            requiresExistingPassword);
}

/// <summary>
/// Nature du résultat d'un retour de fournisseur.
/// </summary>
public enum ExternalCallbackStatus
{
    /// <summary>
    /// Session locale ouverte.
    /// </summary>
    SignedIn = 0,

    /// <summary>
    /// Un mot de passe est nécessaire avant d'ouvrir la session.
    /// </summary>
    PasswordRequired = 1,
}
